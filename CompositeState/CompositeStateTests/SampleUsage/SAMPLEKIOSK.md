# Sample Kiosk application

Sample application is a simple kiosk application. It provides a few different modes of operation, with specific permitted transitions between discrete states.

Below are different representations of a desired StateMachine.

## State Transition Table representation

This is a [One-Dimension State Transition Table](https://en.wikipedia.org/wiki/State-transition_table#One-dimension). This is perhaps easiest to understand from a computational perspective; I have a value of a current state, I have been given an input, and determining next state is a trivial look up. Unfortunately, this is not particularly intuitive. Finding errors or missing transitions may be difficult and requires a careful audit. Furthermore, there is no concept of "sub-state" or re-usable StateMachine. We emulate sub-states by prefixing clustered states with a common term (eg "PublicPhotoSession." or "PersonalizedPhotoSession.") but authoring this table requires enumerating each distinct state. There are no time-savings or re-usability in this model.

| Input | Current state | Next state |
| ----- | ------------- | ---------- |
| Continue | Startup | Login |
| Public | Login | PublicPhotoSession.Welcome |
| Personalized | Login | PersonalizedPhotoSession.Welcome |
| Administrator | Login | AdministratorSession.Menu |
| Timeout | Login | Idle |
| Continue | PublicPhotoSession.Welcome | PublicPhotoSession.CapturePhoto |
| Timeout | PublicPhotoSession.Welcome | Login |
| Logout | PublicPhotoSession.Welcome | Login |
| Continue | PublicPhotoSession.CapturePhoto | PublicPhotoSession.EditPhoto |
| Timeout | PublicPhotoSession.CapturePhoto | Login |
| Logout | PublicPhotoSession.CapturePhoto | Login |
| Continue | PublicPhotoSession.EditPhoto | PublicPhotoSession.PrintPhoto |
| Timeout | PublicPhotoSession.EditPhoto | Login |
| Logout | PublicPhotoSession.EditPhoto | Login |
| Continue | PublicPhotoSession.PrintPhoto | PublicPhotoSession.ThankYou |
| Repeat | PublicPhotoSession.PrintPhoto | PublicPhotoSession.CapturePhoto |
| Timeout | PublicPhotoSession.PrintPhoto | Login |
| Logout | PublicPhotoSession.PrintPhoto | Login |
| Continue | PublicPhotoSession.ThankYou | Login |
| Timeout | PublicPhotoSession.ThankYou | Login |
| Logout | PublicPhotoSession.ThankYou | Login |
| Continue | PersonalizedPhotoSession.Welcome | PersonalizedPhotoSession.CapturePhoto |
| Timeout | PersonalizedPhotoSession.Welcome | Login |
| Logout | PersonalizedPhotoSession.Welcome | Login |
| Continue | PersonalizedPhotoSession.CapturePhoto | PersonalizedPhotoSession.EditPhoto |
| Timeout | PersonalizedPhotoSession.CapturePhoto | Login |
| Logout | PersonalizedPhotoSession.CapturePhoto | Login |
| Continue | PersonalizedPhotoSession.EditPhoto | PersonalizedPhotoSession.PrintPhoto |
| Timeout | PersonalizedPhotoSession.EditPhoto | Login |
| Logout | PersonalizedPhotoSession.EditPhoto | Login |
| Continue | PersonalizedPhotoSession.PrintPhoto | PersonalizedPhotoSession.ThankYou |
| Repeat | PersonalizedPhotoSession.PrintPhoto | PersonalizedPhotoSession.CapturePhoto |
| Timeout | PersonalizedPhotoSession.PrintPhoto | Login |
| Logout | PersonalizedPhotoSession.PrintPhoto | Login |
| Continue | PersonalizedPhotoSession.ThankYou | Login |
| Timeout | PersonalizedPhotoSession.ThankYou | Login |
| Logout | PersonalizedPhotoSession.ThankYou | Login |
| FlushCache | AdministratorSession.Menu | AdministratorSession.FlushCache |
| DeleteHistory | AdministratorSession.Menu | AdministratorSession.DeleteHistory |
| ViewLog | AdministratorSession.Menu | AdministratorSession.ViewLog |
| Timeout | AdministratorSession.Menu | Login |
| Logout | AdministratorSession.Menu | Login |
| Continue | AdministratorSession.FlushCache | AdministratorSession.Menu |
| GoBack | AdministratorSession.FlushCache | AdministratorSession.Menu |
| Timeout | AdministratorSession.FlushCache | Login |
| Logout | AdministratorSession.FlushCache | Login |
| Continue | AdministratorSession.DeleteHistory | AdministratorSession.Menu |
| GoBack | AdministratorSession.DeleteHistory | AdministratorSession.Menu |
| Timeout | AdministratorSession.DeleteHistory | Login |
| Logout | AdministratorSession.DeleteHistory | Login |
| Continue | AdministratorSession.ViewLog | AdministratorSession.Menu |
| GoBack | AdministratorSession.ViewLog | AdministratorSession.Menu |
| Timeout | AdministratorSession.ViewLog | Login |
| Logout | AdministratorSession.ViewLog | Login |
| Continue | Idle | Login |

## Mermaid State Diagram representation

This is a [mermaid State Diagram](https://mermaid-js.github.io/mermaid/#/stateDiagram?id=state-diagrams). This representation does not lend itself easily to computation, but in terms of intuiting or understanding its behavior, this is a marked improvement over State Transition Tables. This visual representation captures sub-states with explicit sub-graphs. While we are able to visualize sub-states as discrete StateMachines we must still author each individually (see code-behind), but this is a step closer toward a desired model.

``` mermaid
stateDiagram-v2
    [*] --> Startup 
    Startup --> Login : Continue
    Login --> PublicPhotoSession : Public
    Login --> PersonalizedPhotoSession : Personalized
    Login --> AdministratorSession : Administrator
    Login --> Idle : Timeout
    PublicPhotoSession --> Login : Continue
    PublicPhotoSession --> Login : Timeout
    PublicPhotoSession --> Login : Logout
    PersonalizedPhotoSession --> Login : Continue
    PersonalizedPhotoSession --> Login : Timeout
    PersonalizedPhotoSession --> Login : Logout
    AdministratorSession --> Login : Timeout
    AdministratorSession --> Login : Logout
    Idle --> Login : Continue

    state PublicPhotoSession
    {
        PublicPhotoSession.Welcome : Welcome
        PublicPhotoSession.CapturePhoto : CapturePhoto
        PublicPhotoSession.EditPhoto : EditPhoto
        PublicPhotoSession.PrintPhoto : PrintPhoto
        PublicPhotoSession.ThankYou : ThankYou
        [*] --> PublicPhotoSession.Welcome
        PublicPhotoSession.Welcome --> PublicPhotoSession.CapturePhoto : Continue
        PublicPhotoSession.CapturePhoto --> PublicPhotoSession.EditPhoto : Continue
        PublicPhotoSession.EditPhoto --> PublicPhotoSession.PrintPhoto : Continue
        PublicPhotoSession.PrintPhoto --> PublicPhotoSession.CapturePhoto : Repeat
        PublicPhotoSession.PrintPhoto --> PublicPhotoSession.ThankYou : Continue
        PublicPhotoSession.ThankYou --> [*]
    }

    state PersonalizedPhotoSession
    {
        PersonalizedPhotoSession.Welcome : Welcome
        PersonalizedPhotoSession.CapturePhoto : CapturePhoto
        PersonalizedPhotoSession.EditPhoto : EditPhoto
        PersonalizedPhotoSession.PrintPhoto : PrintPhoto
        PersonalizedPhotoSession.ThankYou : ThankYou
        [*] --> PersonalizedPhotoSession.Welcome
        PersonalizedPhotoSession.Welcome --> PersonalizedPhotoSession.CapturePhoto : Continue
        PersonalizedPhotoSession.CapturePhoto --> PersonalizedPhotoSession.EditPhoto : Continue
        PersonalizedPhotoSession.EditPhoto --> PersonalizedPhotoSession.PrintPhoto : Continue
        PersonalizedPhotoSession.PrintPhoto --> PersonalizedPhotoSession.CapturePhoto : Repeat
        PersonalizedPhotoSession.PrintPhoto --> PersonalizedPhotoSession.ThankYou : Continue
        PersonalizedPhotoSession.ThankYou --> [*]
    }

    state AdministratorSession
    {
        direction LR
        AdministratorSession.Menu : Menu
        AdministratorSession.FlushCache : FlushCache
        AdministratorSession.DeleteHistory : DeleteHistory
        AdministratorSession.ViewLog : ViewLog
        [*] --> AdministratorSession.Menu
        AdministratorSession.Menu --> AdministratorSession.FlushCache : FlushCache
        AdministratorSession.Menu --> AdministratorSession.DeleteHistory : DeleteHistory
        AdministratorSession.Menu --> AdministratorSession.ViewLog : ViewLog
        AdministratorSession.FlushCache --> AdministratorSession.Menu : Continue
        AdministratorSession.FlushCache --> AdministratorSession.Menu : GoBack
        AdministratorSession.DeleteHistory --> AdministratorSession.Menu : Continue
        AdministratorSession.DeleteHistory --> AdministratorSession.Menu : GoBack
        AdministratorSession.ViewLog --> AdministratorSession.Menu : Continue
        AdministratorSession.ViewLog --> AdministratorSession.Menu : GoBack
        AdministratorSession.Menu --> [*]
    }
```