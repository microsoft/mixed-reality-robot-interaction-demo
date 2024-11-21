# The components of this demo that depend on the "Azure Spatial Anchors (ASA)" service no longer work, as that service has been retired on November 20th 2024 (see [announcement](https://azure.microsoft.com/en-us/updates/azure-spatial-anchors-retirement/)). This repo will remain public so that the other components remain available to the community.

# IROS 2020 Mixed Reality and Robotics Tutorial
# Demo 1: Robot Interaction through Mixed Reality

Microsoft is hosting a workshop at the IROS 2020 conference on Mixed Reality and Robotics.
This repository contains all the code required to run, build and deploy Demo 1.
The tutorial _was_ available from October 2020 until January 2021 at https://www.iros2020.org/.

The lecture content of the tutorial is now available as a Microsoft Research Webinar: https://note.microsoft.com/MSR-Webinar-Reality-and-Robotics-Registration-On-Demand.html

The hands-on video walkthroughs for the demos are available on the MSR YouTube channel:
- [Demo 1: Interaction](https://youtu.be/4G3wjPIs4Fc) (the demo that goes with the code in this repo)
- [Demo 2: Colocalization](https://youtu.be/P11LcMOp2CE) (the demo that uses the [ASA ROS Wrapper](https://github.com/microsoft/azure_spatial_anchors_ros/) and is described in the [wiki](https://github.com/microsoft/azure_spatial_anchors_ros/wiki/Demo) there)

Demo 1 aims to:
- Highlight how the robotic community can benefit from Mixed Reality
- Show some features of the HoloLens 2, Azure Spatial Anchors, and other products, services & tools from the Mixed Reality ecosystem.
- Serve as a playground and starting point for researchers to use Mixed Reality with their own robots

The demo application allows you to command a simulated or real Clearpath Jackal robot and an industrial robot arm attached to it in MR. There are multiple control modes available. This repo also contains Dockerfiles to build and run the Jackal simulation and navigation stack.

# Instructions
**Please see [the Wiki](https://github.com/microsoft/mixed-reality-robot-interaction-demo/wiki) for detailed instructions**.
The wiki contains detailed instructions on how to run, build and deploy the demo apps. The videos accompanying this repository are currently available through [IROS2020](https://www.iros2020.org/).

# Support
If you have questions or issues while working with this repository, please open an issue [here](https://github.com/microsoft/mixed-reality-robot-interaction-demo/issues).

## Contributing
This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks
This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft 
trademarks or logos is subject to and must follow [Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.
