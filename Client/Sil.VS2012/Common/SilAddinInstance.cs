using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using SilAPI;
using SilUI;

namespace Sil.VSCommon
{
    /// <summary>
    /// Represents the Sil addin instance.
    /// </summary>
    public class SilAddinInstance : IDisassemblyProvider
    {
        private readonly DTE2 application;
        private readonly AddIn addIn;
        private CommandBarControl controlCodeMenuDisassembleCommand;

        private const string Command_SeeIL_CommandName = @"SeeIntermediateLanguage";
        private const string Command_SeeIL_Caption = @"Disassemble";
        private const string Command_SeeIL_Tooltip = @"Disassemble the selection";

        /// <summary>
        /// Initializes a new instance of the <see cref="SilAddinInstance"/> class.
        /// </summary>
        /// <param name="application">The application.</param>
        /// <param name="addIn">The add in.</param>
        public SilAddinInstance(DTE2 application, AddIn addIn)
        {
            this.application = application;
            this.addIn = addIn;
        }

        /// <summary>
        /// Creates the commands.
        /// </summary>
        public void CreateCommands()
        {
            try
            {
                var contextUIGuids = new object[] { };

                //  Create the 'See IL' command.
                application.Commands.AddNamedCommand(addIn, Command_SeeIL_CommandName,
                                                     Command_SeeIL_Caption, Command_SeeIL_Tooltip, false, 1,
                                                     ref contextUIGuids, (int)vsCommandStatus.vsCommandStatusSupported);
            }
            catch
            {
                //  Creating the command should only fail if we have it already.
            }
        }

        /// <summary>
        /// Creates the user interface.
        /// </summary>
        public void CreateUserInterface()
        {
            //  Get the sil command name.
            var silCommandId = addIn.ProgID + "." + Command_SeeIL_CommandName;

            //  If we don't have the command, create it.
            if (application.Commands.OfType<Command>().Any(c => c.Name != silCommandId))
                CreateCommands();

            //  Create the control for the Sil command.
            try
            {
                var commandSil = application.Commands.Item(addIn.ProgID + "." + Command_SeeIL_CommandName);

                //  Retrieve the context menu of code windows.
                var commandBars = (CommandBars)application.CommandBars;
                var codeWindowCommandBar = commandBars["Code Window"];

                //  Clean up duplicate commands.
                CleanUpCodeMenu(codeWindowCommandBar);

                //  Add the one line command.
                controlCodeMenuDisassembleCommand = (CommandBarControl)commandSil.AddControl(codeWindowCommandBar,
                                                                                             codeWindowCommandBar.Controls.Count + 1);
            }
            catch (Exception exception)
            {
                ExceptionHandler.HandleException(@"Failed to create the 'Disassemble' command.", exception);
            }
        }

        /// <summary>
        /// Destroys the user interface.
        /// </summary>
        public void DestroyUserInterface()
        {
            if (controlCodeMenuDisassembleCommand == null) 
                return;
            
            try
            {
                controlCodeMenuDisassembleCommand.Delete();
            }
            catch
            {
                //  The application is shutting down, don't interfere with the user.
            }
        }

        /// <summary>
        /// Cleans up code menu.
        /// </summary>
        /// <param name="codeMenu">The code menu.</param>
        private static void CleanUpCodeMenu(CommandBar codeMenu)
        {
            //  Find any command with the correct name.
            var controlsToCleanUp = codeMenu.Controls.OfType<CommandBarControl>().Where(cbc => cbc.TooltipText == Command_SeeIL_Tooltip).ToList();
            controlsToCleanUp.ForEach(ctc => ctc.Delete());
        }

        /// <summary>
        /// Executes the sil command.
        /// </summary>
        public void ExecuteSilCommand()
        {
            //  Create a new GUID for the window.
            var guid = Guid.NewGuid().ToString();

            //  Create a new object to hold the control object.
            var controlObject = new object();

            //  Get the application windows.
            var windows = (Windows2)application.Windows;

            //  Get the type of the Sil View Host.
            var silViewHostType = typeof(SilViewHostWrapper);

            //  Get the view host assembly and class name.
            var hostAssemblyPath = silViewHostType.Assembly.Location;
            var hostClassName = silViewHostType.FullName;

            //  Identify the disassembly targets.
            var targets = IdentifyDisassemblyTargets().ToList();

            //  Create the window.
            var window = (Window2)windows.CreateToolWindow2(addIn, hostAssemblyPath,
                                                            hostClassName, "Sil", guid, ref controlObject);

            //  Get the sil host.
            var host = (SilViewHostWrapper)controlObject;

            //  Initialise the host with the parent window.
            host.InitialiseParentWindow(window);

            //  Initialise the host.
            host.SilViewHost.InitialiseView(this, targets.FirstOrDefault());

            //  Ensure that the window is visible.
            window.Visible = true;
        }

        /// <summary>
        /// Queries the command status.
        /// </summary>
        /// <param name="cmdName">Name of the CMD.</param>
        /// <param name="neededText">The needed text.</param>
        /// <param name="statusOption">The status option.</param>
        /// <param name="commandText">The command text.</param>
        public void QueryCommandStatus(string cmdName, vsCommandStatusTextWanted neededText, ref vsCommandStatus statusOption, ref object commandText)
        {
            if (cmdName == addIn.ProgID + "." + Command_SeeIL_CommandName)
            {
                // ReSharper disable BitwiseOperatorOnEnumWithoutFlags
                statusOption = vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
                // ReSharper restore BitwiseOperatorOnEnumWithoutFlags
            }
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="cmdName">Name of the CMD.</param>
        /// <param name="executeOption">The execute option.</param>
        /// <param name="variantIn">The variant in.</param>
        /// <param name="variantOut">The variant out.</param>
        /// <param name="handled">if set to <c>true</c> [handled].</param>
        public void ExecuteCommand(string cmdName, vsCommandExecOption executeOption, ref object variantIn, ref object variantOut, ref bool handled)
        {
            if (cmdName == addIn.ProgID + "." + Command_SeeIL_CommandName)
            {
                ExecuteSilCommand();
            }
        }

        /// <summary>
        /// Identifies the disassembly targets.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<DisassemblyTarget> IdentifyDisassemblyTargets()
        {
            //  Get the active point.
            var textSelection = (TextSelection)application.ActiveDocument.Selection;
            var activePoint = textSelection.ActivePoint;

            //  We'll check for these targets in order.
            var element = activePoint.CodeElement[vsCMElement.vsCMElementVariable];
            if (element != null)
                yield return new DisassemblyTarget(DisassemblyTargetType.Field, element.FullName);

            element = activePoint.CodeElement[vsCMElement.vsCMElementEvent];
            if (element != null)
                yield return new DisassemblyTarget(DisassemblyTargetType.Event, element.FullName);

            element = activePoint.CodeElement[vsCMElement.vsCMElementDelegate];
            if (element != null)
                yield return new DisassemblyTarget(DisassemblyTargetType.Delegate, element.FullName);

            element = activePoint.CodeElement[vsCMElement.vsCMElementFunction];
            if (element != null)
                yield return new DisassemblyTarget(DisassemblyTargetType.Method, element.FullName);

            element = activePoint.CodeElement[vsCMElement.vsCMElementProperty];
            if (element != null)
                yield return new DisassemblyTarget(DisassemblyTargetType.Property, element.FullName);

            element = activePoint.CodeElement[vsCMElement.vsCMElementClass];
            if (element != null)
                yield return new DisassemblyTarget(DisassemblyTargetType.Class, element.FullName);

            element = activePoint.CodeElement[vsCMElement.vsCMElementInterface];
            if (element != null)
                yield return new DisassemblyTarget(DisassemblyTargetType.Interface, element.FullName);

            element = activePoint.CodeElement[vsCMElement.vsCMElementStruct];
            if (element != null)
                yield return new DisassemblyTarget(DisassemblyTargetType.Structure, element.FullName);
        }

        /// <summary>
        /// Disassembles the assembly.
        /// </summary>
        /// <returns>
        /// A task to disassemble the assembly.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">There is no current project to disassemble.</exception>
        Task<DisassembledAssembly> IDisassemblyProvider.DisassembleAssembly()
        {
            //  We need the project.
            if (application.ActiveDocument == null ||
                application.ActiveDocument.ProjectItem == null ||
                application.ActiveDocument.ProjectItem.ContainingProject == null)
                throw new InvalidOperationException("There is no current project to disassemble.");

            var activeProject = application.ActiveDocument.ProjectItem.ContainingProject;

            //  From the active project, get the output path.
            var activeConfig = activeProject.ConfigurationManager.ActiveConfiguration;

            //  Get the path.
            var FullPath = activeProject.Properties.Item("FullPath").Value.ToString();
            var InputAssembly = activeConfig.Properties.Item("CodeAnalysisInputAssembly").Value.ToString();
            var assemblyPath = Path.Combine(FullPath, InputAssembly);

            return Task<DisassembledAssembly>.Factory.StartNew(() => Disassembler.DisassembleAssembly(assemblyPath));
        }
    }
}