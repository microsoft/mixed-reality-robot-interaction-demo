# Base image used to create costum ROS1 containers
FROM iros2020.azurecr.io/jackal_base:latest


# Setting default shell to bash
SHELL ["/bin/bash", "--login", "-c"]

RUN sudo apt-get install -y apt-utils
RUN mkdir -p /home/catkin_ws/src

##################
# install Jackal #
##################
# RUN sudo apt-get install ros-melodic-jackal-simulator ros-melodic-jackal-desktop ros-melodic-jackal-navigation -y
# Note: jackal is now installed in the base image already.

##############################################
# Install python etc. for Autobahn websocket #
##############################################
RUN apt-get update
RUN apt install -y python3-pip
RUN apt-get install -y python-pip 
RUN pip install autobahn tornado bson pymongo pillow

#####################
# install RosBridge #
#####################
RUN apt-get -y install ros-melodic-rosbridge-server 

########################
# download file_server #
########################
RUN mkdir -p /home/catkin_ws/src && cd /home/ && git clone https://github.com/EricVoll/ros-sharp.git
RUN mv /home/ros-sharp/ROS/file_server /home/catkin_ws/src
RUN rm -r -f /home/ros-sharp

###########################
# download ms_jackal_demo #
###########################
RUN cd /home/ && git clone https://github.com/EricVoll/jackal_description
RUN mv /home/jackal_description/docker/resources/packages/ms_jackal_demo /home/catkin_ws/src
RUN rm -r -f /home/jackal_description

#########################
# install catkin simple #
#########################

RUN source /opt/ros/melodic/setup.bash && \
        cd /home/catkin_ws/src && \
        git clone https://github.com/catkin/catkin_simple && \
        cd /home/catkin_ws && \
        catkin_make

############################################
# call catkin make to install the packages #
############################################
RUN source /opt/ros/melodic/setup.bash && \
        cd /home/catkin_ws && \
        catkin_make && \
        source /home/catkin_ws/devel/setup.bash && \
        echo 'source /home/catkin_ws/devel/setup.bash' >> ~/.bashrc


