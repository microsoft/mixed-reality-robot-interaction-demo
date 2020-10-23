cmake_minimum_required(VERSION 3.0.2)
project(ms_jackal_demo)

###################
## Python Script ##
###################

find_package(catkin_simple REQUIRED)
catkin_simple(ALL_DEPS_REQUIRED)
cs_install()
cs_install_scripts(
  scripts/tf_transform.py
)
cs_export()
