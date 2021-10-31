# ComposableState

[[_TOC_]]

## Introduction

ComposableState is an OpenSource nuget project for an implementation of Composable StateMachines; ie a StateMachine is composed of States, and States may in turn be a StateMachine. Primary drivers:
1. a configuration that more closely aligns with traditional StateMachine literature (eg [State Transition Tables](https://en.wikipedia.org/wiki/State-transition_table#One-dimension)), and
1. configuration that may be re-used as SubStates
