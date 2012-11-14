using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Clide
{
	/// <summary>
	/// Interface used by settings that leverage the <see cref="ToolsOptionsPage{TControl, TSettings}"/> 
	/// base class for Tools|Options extensibility, and which are annotated with the <see cref="SettingsAttribute"/>.
	/// </summary>
	public interface ISettings : IEditableObject
	{
	}
}
