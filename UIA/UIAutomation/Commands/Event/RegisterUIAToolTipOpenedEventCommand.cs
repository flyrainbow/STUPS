﻿/*
 * Created by SharpDevelop.
 * User: Alexander Petrovskiy
 * Date: 4/16/2012
 * Time: 12:33 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

namespace UIAutomation.Commands
{
    extern alias UIANET;using System.Windows.Automation;
    using System.Management.Automation;
    using classic = UIANET::System.Windows.Automation; // using System.Windows.Automation;
    using UIAutomation.Helpers.Commands;

    /// <summary>
    /// Description of RegisterUiaToolTipOpenedEventCommand.
    /// </summary>
    [Cmdlet(VerbsLifecycle.Register, "UiaToolTipOpenedEvent")]
    public class RegisterUiaToolTipOpenedEventCommand : EventCmdletBase
    {
        public RegisterUiaToolTipOpenedEventCommand()
        {
            AutomationEventType = AutomationElement.ToolTipOpenedEvent;
            AutomationEventHandler = OnUIAutomationEvent;
        }
    }
}
