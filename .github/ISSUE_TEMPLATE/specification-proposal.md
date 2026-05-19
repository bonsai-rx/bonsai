---
name: Create a language specification
about: For proposals that have been invited by a team member
title: 'Feature name'
labels: proposal
assignees: ''

---

<!--
Hello, and thanks for your interest in contributing to the Bonsai visual reactive programming language! If you haven't been invited by a team member to open an issue, please instead open a discussion marked [proposal] at https://github.com/bonsai-rx/bonsai/discussions and we'll try to give you feedback on how to get to an issue-ready proposal.

New language proposals should aim to fully fill out this template, at least up to and including detailed design. The sections on drawbacks, alternatives and unresolved questions may be omitted from the initial proposal.
-->

* [x] Proposed
* [ ] Prototype: Not Started
* [ ] Implementation: Not Started
* [ ] Specification: Not Started

## Summary

<!-- One paragraph explanation of the feature. -->

## Motivation

<!-- Why are we doing this? What use cases does it support? What is the expected outcome? -->

## Detailed Design

<!-- This is the bulk of the proposal. The Bonsai visual reactive programming language spans several levels: the compiler (Bonsai.Core), which handles workflow compilation, runtime execution, scope rules, and .bonsai file serialization; the editor (Bonsai.Editor and Bonsai.Design), which provides the visual programming environment, visualizers, and property grid; the standard library and the broader package ecosystem, including packaging, versioning, and distribution; and the CLI and bootstrapper (Bonsai executable). Changes to the language might impact any or all of these levels, so please make sure you have considered the impact of your proposal on each of them.

Explain the design in enough detail for somebody familiar with Bonsai to understand, and for somebody familiar with the impacted levels to implement, and include examples of how the feature will be used. Please include links to relevant parts of the existing language to describe the changes necessary to implement this feature. An initial proposal does not need to cover all cases, but it should have enough detail to enable a team member to bring this proposal to design if they so choose. -->

### Workflow File Compatibility

<!-- How does this proposal interact with workflows saved in existing .bonsai files? Are existing workflows still loadable after this change? Are workflows saved by Bonsai versions including this feature loadable in older Bonsai versions, and if not, what is the failure mode? Document any migration path users may need to follow. -->

### Editor Experience

<!-- How does this feature manifest in the visual editor? How is it represented visually? How is it discovered (toolbox, search, autocomplete)? How do users interact with it (drag and drop, externalization, opening nested workflows, property grid)? Is the visual representation distinct from existing operators or constructs it might be confused with? -->

## Drawbacks

<!-- Why should we *not* do this? -->

## Alternatives

<!-- What other designs have been considered? What is the impact of not doing this? -->

## Unresolved Questions

<!-- What parts of the design are still undecided? -->

## Design Meetings

<!-- Link to bonsai-rx developer meetings or threads where this proposal has been discussed. -->
