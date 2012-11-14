using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Clide
{
	/// <summary>
	/// Interface that exists to abstract the underlying VS settings store to make the functionality of the settings manager 
	/// testable.
	/// </summary>
	internal interface ISettingsStore
	{
		bool CollectionExists(string collectionPath);

		void DeleteCollection(string collectionPath);

		void CreateCollection(string collectionPath);

		bool PropertyExists(string collectionPath, string propertyName);

		void DeleteProperty(string collectionPath, string propertyName);

		void SetString(string collectionPath, string propertyName, string stringValue);

		string GetString(string collectionPath, string propertyName);
	}
}
