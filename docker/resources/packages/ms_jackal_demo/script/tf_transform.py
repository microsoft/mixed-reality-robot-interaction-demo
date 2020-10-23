#!/usr/bin/env python
import rospy
from geometry_msgs.msg import TransformStamped
import tf2_ros

def tf_transform_publisher():
  rospy.init_node('tf_transform_publisher', anonymous=False)

  # TF interface
  tf_buffer = tf2_ros.Buffer()
  tf_listener = tf2_ros.TransformListener(tf_buffer)

  # Publishers and subscribers
  pub = rospy.Publisher('transform_output', TransformStamped, queue_size=1, latch=True)

  # ROS params
  parent_frame = rospy.get_param('~parent_frame', 'anchor')
  child_frame = rospy.get_param('~child_frame', 'odom')
  update_rate = rospy.get_param('~update_rate', 10.0)

  rate = rospy.Rate(update_rate)
  while not rospy.is_shutdown():
    try:
      # Will get the latest transform
      trans = tf_buffer.lookup_transform(parent_frame, child_frame, rospy.Time())
    except (tf2_ros.LookupException, tf2_ros.ConnectivityException, tf2_ros.ExtrapolationException):
      # Just wait if we can't look up the transform.
      rate.sleep()
      continue
    pub.publish(trans)
    rate.sleep()

if __name__ == '__main__':
  try:
    tf_transform_publisher()
  except rospy.ROSInterruptException:
    pass
