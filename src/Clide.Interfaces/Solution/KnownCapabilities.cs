using System;

namespace Clide
{
    [Flags]
    public enum KnownCapabilities
    {
        /// <summary>
        /// The project is a shared assets project
        /// </summary>
        SharedAssetsProject = 1,

        /// <summary>
        /// The project supports the .NET runtime.
        /// </summary>
        NET = 2,

        /// <summary>
        /// The project builds managed code.
        /// </summary>
        Managed = 4,

        /// <summary>
        /// The project supports the C# language.
        /// </summary>
        CSharp = 8,

        /// <summary>
        /// The project supports the VB language.
        /// </summary>
        VB = 16,

        /// <summary>
        /// The project supports Windows Universal XAML (i.e. it's a UWP app).
        /// </summary>
        WindowsXaml,

        /// <summary>
        /// The project supports Xamarin XAML (i.e. it's a Xamarin Android or iOS 
        /// app or library).
        /// </summary>
        XamarinXaml,

        /// <summary>
        /// The project has the Xamarin.Forms nuget package installed and restored 
        /// (important since the capability comes from an imported targets file).
        /// </summary>
        XamarinForms,
    }
}