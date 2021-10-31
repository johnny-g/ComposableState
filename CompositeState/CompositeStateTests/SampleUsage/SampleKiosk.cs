namespace CompositeState
{

    /// <summary>
    /// Common application-wide inputs.
    /// </summary>
    public enum ApplicationInput
    {
        Continue,
        GoBack,
        Cancel,
        Repeat,
        Logout,
        Timeout,
    }

    /// <summary>
    /// Login inputs. Specifically, Login report a "choice" that does not 
    /// trivially map to our application-wide inputs. In this sample kiosk
    /// a user may attempt an "anonymous" session with no personalization with 
    /// 'Public', may log in with a previously created account for some 
    /// personalized interactions, or may log in as an Administrator to manage
    /// application state.
    /// </summary>
    public enum LoginInput
    {
        Public,
        Personalized,
        Administrator,
    }

    /// <summary>
    /// Administrator inputs. Similar "choice" that does not map to application
    /// inputs.
    /// </summary>
    public enum AdministratorInput
    {
        FlushCache,
        DeleteHistory,
        ViewLog,
    }

    /// <summary>
    /// High-level operational states. Startup to Login where we exercise a 
    /// choice for user activities (Public, Personalized, or Administrator) with
    /// an Idle sink if there is no activity.
    /// </summary>
    public enum KioskState
    {
        Startup,
        Login,
        PublicPhotoSession,
        PersonalizedPhotoSession,
        AdministratorSession,
        Idle,
    }

    /// <summary>
    /// Photo activity states. Fairly linear progression, with an option to 
    /// repeat activity.
    /// </summary>
    public enum PhotoState
    {
        Welcome,
        CapturePhoto,
        EditPhoto,
        PrintPhoto,
        ThankYou,
    }

    /// <summary>
    /// Standard menu.
    /// </summary>
    public enum AdministratorState
    {
        Menu,
        FlushCache,
        DeleteHistory,
        ViewLog,
    }

    public class SampleKiosk
    {
        public void Configuration()
        {

            StateMachineConfiguration photo = new StateMachineConfiguration
            {
                Start = PhotoState.Welcome,
                States = new[]
                {
                    new StateConfiguration
                    {
                        State = PhotoState.Welcome,
                        Transitions = new[]
                        {
                            new TransitionConfiguration { Input = ApplicationInput.Continue, Next = PhotoState.CapturePhoto, },
                        },
                    },
                    new StateConfiguration
                    {
                        State = PhotoState.CapturePhoto,
                        Transitions = new[]
                        {
                            new TransitionConfiguration { Input = ApplicationInput.Continue, Next = PhotoState.EditPhoto, },
                        },
                    },
                    new StateConfiguration
                    {
                        State = PhotoState.EditPhoto,
                        Transitions = new[]
                        {
                            new TransitionConfiguration { Input = ApplicationInput.Continue, Next = PhotoState.PrintPhoto, },
                        },
                    },
                    new StateConfiguration
                    {
                        State = PhotoState.PrintPhoto,
                        Transitions = new[]
                        {
                            new TransitionConfiguration { Input = ApplicationInput.Continue, Next = PhotoState.ThankYou, },
                            new TransitionConfiguration { Input = ApplicationInput.Repeat, Next = PhotoState.CapturePhoto, },
                        },
                    },
                    new StateConfiguration { State = PhotoState.ThankYou, },
                },
            };

            StateMachineConfiguration administrator = new StateMachineConfiguration
            {
                Start = AdministratorState.Menu,
                States = new[]
                {
                    new StateConfiguration
                    {
                        State = AdministratorState.Menu,
                        Transitions = new[]
                        {
                            new TransitionConfiguration { Input = AdministratorInput.FlushCache, Next = AdministratorState.FlushCache, },
                            new TransitionConfiguration { Input = AdministratorInput.DeleteHistory, Next = AdministratorState.DeleteHistory, },
                            new TransitionConfiguration { Input = AdministratorInput.ViewLog, Next = AdministratorState.ViewLog, },
                        },
                    },
                    new StateConfiguration
                    {
                        State = AdministratorState.FlushCache,
                        Transitions = new[]
                        {
                            new TransitionConfiguration { Input = ApplicationInput.Continue, Next = AdministratorState.Menu, },
                            new TransitionConfiguration { Input = ApplicationInput.GoBack, Next = AdministratorState.Menu, },
                        },
                    },
                    new StateConfiguration
                    {
                        State = AdministratorState.DeleteHistory,
                        Transitions = new[]
                        {
                            new TransitionConfiguration { Input = ApplicationInput.Continue, Next = AdministratorState.Menu, },
                            new TransitionConfiguration { Input = ApplicationInput.GoBack, Next = AdministratorState.Menu, },
                        },
                    },
                    new StateConfiguration
                    {
                        State = AdministratorState.ViewLog,
                        Transitions = new[]
                        {
                            new TransitionConfiguration { Input = ApplicationInput.Continue, Next = AdministratorState.Menu, },
                            new TransitionConfiguration { Input = ApplicationInput.GoBack, Next = AdministratorState.Menu, },
                        },
                    },
                },
            };

            StateMachineConfiguration kiosk = new StateMachineConfiguration
            {
                Start = KioskState.Startup,
                States = new[]
                {
                    new StateConfiguration
                    {
                        State = KioskState.Startup,
                        Transitions = new[]
                        {
                            new TransitionConfiguration { Input = ApplicationInput.Continue, Next = KioskState.Login, },
                        },
                    },
                    new StateConfiguration
                    {
                        State = KioskState.Login,
                        Transitions = new[]
                        {
                            new TransitionConfiguration { Input = LoginInput.Public, Next = KioskState.PublicPhotoSession, },
                            new TransitionConfiguration { Input = LoginInput.Personalized, Next = KioskState.PersonalizedPhotoSession, },
                            new TransitionConfiguration { Input = LoginInput.Administrator, Next = KioskState.AdministratorSession, },
                            new TransitionConfiguration { Input = ApplicationInput.Timeout, Next = KioskState.Idle, },
                        },
                    },
                    new StateConfiguration
                    {
                        State = KioskState.PublicPhotoSession,
                        SubState = photo,
                        Transitions = new[]
                        {
                            new TransitionConfiguration { Input = ApplicationInput.Continue, Next = KioskState.Login, },
                            new TransitionConfiguration { Input = ApplicationInput.Logout, Next = KioskState.Login, },
                            new TransitionConfiguration { Input = ApplicationInput.Timeout, Next = KioskState.Login, },
                        },
                    },
                    new StateConfiguration
                    {
                        State = KioskState.PersonalizedPhotoSession,
                        SubState = photo,
                        Transitions = new[]
                        {
                            new TransitionConfiguration { Input = ApplicationInput.Continue, Next = KioskState.Login, },
                            new TransitionConfiguration { Input = ApplicationInput.Logout, Next = KioskState.Login, },
                            new TransitionConfiguration { Input = ApplicationInput.Timeout, Next = KioskState.Login, },
                        },
                    },
                    new StateConfiguration
                    {
                        State = KioskState.AdministratorSession,
                        SubState = administrator,
                        Transitions = new[]
                        {
                            new TransitionConfiguration { Input = ApplicationInput.Logout, Next = KioskState.Login, },
                            new TransitionConfiguration { Input = ApplicationInput.Timeout, Next = KioskState.Login, },
                        },
                    },
                    new StateConfiguration
                    {
                        State = KioskState.Idle,
                        Transitions = new[]
                        {
                            new TransitionConfiguration { Input = ApplicationInput.Continue, Next = KioskState.Login, },
                        },
                    },
                },
            };

        }
    }

}