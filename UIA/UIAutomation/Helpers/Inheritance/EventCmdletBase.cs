﻿/*
 * Created by SharpDevelop.
 * User: Alexander Petrovskiy
 * Date: 20/01/2012
 * Time: 09:29 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

namespace UIAutomation
{
    using System;
    using System.Management.Automation;
    using System.Windows.Automation;
    using System.Collections;

    /// <summary>
    /// Description of HasScriptBlockCmdletBase.
    /// </summary>
    //[Cmdlet(VerbsLifecycle.Register, "UIADummyEvent")]
    public class EventCmdletBase : HasControlInputCmdletBase
    {
        #region Constructor
        public EventCmdletBase()
        {
            this.InputObject = 
                new System.Windows.Automation.AutomationElement[] { CurrentData.CurrentWindow };
            this.AutomationEventType = null;
            this.AutomationProperty = null;
            this.AutomationEventHandler = null;
            this.AutomationPropertyChangedEventHandler = null;
            this.StructureChangedEventHandler = null;
        }
        #endregion Constructor

        #region Parameters
        [Parameter(Mandatory = false)]
        internal new SwitchParameter OnErrorScreenShot { get; set; }

        [Parameter(Mandatory = false)]
        public Hashtable[] SearchCriteria { get; set; }
        #endregion Parameters
        
        #region Properties
        #endregion Properties
        
        /// <summary>
        /// Processes the pipeline.
        /// </summary>
        protected override void ProcessRecord()
        {
            if (this.InputObject == null) return;
            
            foreach (AutomationElement inputObject in this.InputObject) {

            SubscribeToEvents(this,
                              inputObject,
                              this.AutomationEventType,
                              this.AutomationProperty);

            } // 20120824
        }
    }
}