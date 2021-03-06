![neodroid](.github/images/header.png)

# Droid

Droid is a unity package that enables prototyping reinforcement learning environments within the [Unity](https://unity3d.com/) engine and communication to the [Neo](https://github.com/sintefneodroid/neo) counterpart of the [Neodroid](https://github.com/sintefneodroid) platform.

---

_[Neodroid](https://github.com/sintefneodroid) is developed with support from Research Council of Norway Grant #262900. ([https://www.forskningsradet.no/prosjektbanken/#/project/NFR/262900](https://www.forskningsradet.no/prosjektbanken/#/project/NFR/262900))_

---


<table>
  <tr>
    <td>
      <a href='https://travis-ci.org/sintefneodroid/droid'>
        <img src='https://travis-ci.org/sintefneodroid/droid.svg?branch=master' alt='Build Status' />
      </a>
    </td>
    <td>
      <a href='https://coveralls.io/github/sintefneodroid/droid?branch=master'>
        <img src='https://coveralls.io/repos/github/sintefneodroid/droid/badge.svg?branch=master' alt='Coverage Status' />
      </a>
    </td>
    <td>
      <a href='https://github.com/sintefneodroid/droid/issues'>
        <img src='https://img.shields.io/github/issues/sintefneodroid/droid.svg?style=flat' alt='GitHub Issues' />
      </a>
    </td>
    <td>
      <a href='https://github.com/sintefneodroid/droid/network'>
        <img src='https://img.shields.io/github/forks/sintefneodroid/droid.svg?style=flat' alt='GitHub Forks' />
      </a>
    </td>
      <td>
      <a href='https://github.com/sintefneodroid/droid/stargazers'>
        <img src='https://img.shields.io/github/stars/sintefneodroid/droid.svg?style=flat' alt='GitHub Stars' />
      </a>
    </td>
      <td>
      <a href='https://github.com/sintefneodroid/droid/blob/master/LICENSE.md'>
        <img src='https://img.shields.io/github/license/sintefneodroid/droid.svg?style=flat' alt='GitHub License' />
      </a>
    </td>
  </tr>
</table>

<p align="center" width="100%">
  <a href="https://unity3d.com/">
    <img alt="unity" src=".github/images/unity.svg" height="40" align="left">
  </a>
  <a href="https://docs.microsoft.com/en-us/dotnet/csharp/index">
    <img alt="csharp" src=".github/images/csharp.svg" height="40" align="center">
  </a>
  <a href="https://github.com/zeromq/netmq">
    <img alt="netmq" src=".github/images/netmq.svg" height="40" align="right">
  </a>
</p>
<p align="center" width="100%">
  <a href="https://github.com/google/flatbuffers">
    <img alt="flatbuffers" src=".github/images/flatbuffers.svg" height="40" align="center">
  </a>
</p>

This project has similarities with Unity's own project [Unity Machine Learning Agents](https://github.com/Unity-Technologies/ml-agents). Most of the efforts done in this project were made prior to their announcement, [Introducing: Unity Machine Learning Agents](https://blogs.unity3d.com/2017/09/19/introducing-unity-machine-learning-agents/), when the authors was in need of a capable tool. Newcomers wanting a more supported experience may wish to use the [Unity Machine Learning Agents](https://github.com/Unity-Technologies/ml-agents) project instead.

The entire Neodroid platform serves as a tool for academic research specific to the authors interests, hence explaining the existence and future direction of this project.

## Notable Features

- In-editor simulations for ease of debugging
- Connect multiple external agents (i.e. multiple client computers)
- Blazing fast serialisation <!-- (see [benchmark](.github/BENCHMARK.MD)) -->
- Modular unity style component construction of scenes (enables rapid prototyping of complex
environments and ease of integration with existing projects)
- Support reverse curriculum generation inherently
(Ability to reinitialise any previous seen state or configure new ones)

## Usage

- Download the newest Droid.unitypackage from [releases](https://github.com/sintefneodroid/droid/releases) and import into your Unity project.

***Or***

- Acquire the [Droid (Temporarily down)](http://u3d.as/14cC) package from the built-in asset store of the Unity Editor.

## Demo
<!--![droid](.github/images/neodroid.png)
![lunarlander](.github/images/lunarlander.png)
-->
![manipulator](.github/images/animated.gif)

## Repository Structure
---
<!--    ├  └  ─  │   -->
    sintefneodroid/droid      # This repository
    │
    ├── docs
    │   ├── source            # Documentation files
    │   │
    │   ├── make.bat          # Compile docs
    │   └── Makefile          # ^
    │
    ├── Examples              # Prebuilt Neodroid environments
    │   ├── Assets            # Model checkpoints
    │   │   ├── Neodroid      # Symlinked folder to top-level Neodroid folder
    │   │   ├── SceneAssets   # All scene-specific assets for the prebuilt environments
    │   │   └── Scenes        # All prebuilt environment scenes
    │   │
    │   └── Examples.sln      # C# project file
    │
    ├── .github            # Images and such for this README
    │
    ├── Neodroid              # The Neodroid unity package
    │   ├── Prototyping       # All classes for quick prototyping of observations and actions
    │   │   ├── Actors
    │   │   ├── Evaluation
    │   │   ├── Observers
    │   │   ├── Displayers
    │   │   ├── Configurables
    │   │   └── Motors
    │   │
    │   ├── Environments      # Classes for encapsulating all Neodroid environments
    │   ├── Managers          # Classes for managing the simulation of Neodroid environments
    │   └── Utilities         # Lots of helper functionalities
    │
    ├── LICENSE               # License file (Important but boring)
    └── README.md             # The top-level README
---

# Citation

For citation you may use the following bibtex entry:
````
@misc{neodroid,
  author = {Heider, Christian},
  title = {Neodroid Platform},
  year = {2018},
  publisher = {GitHub},
  journal = {GitHub repository},
  howpublished = {\url{https://github.com/sintefneodroid}},
}
````
# Other Components Of The Neodroid Platform
- [agent](https://github.com/sintefneodroid/agent)
- [neo](https://github.com/sintefneodroid/neo)
