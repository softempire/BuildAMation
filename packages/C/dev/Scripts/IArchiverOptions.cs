#region License
// Copyright 2010-2015 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion // License
namespace C
{
namespace V2
{
namespace DefaultSettings
{
    public static partial class DefaultSettingsExtensions
    {
        public static void Defaults(this C.V2.ICommonArchiverOptions settings, Bam.Core.V2.Module module)
        {
            settings.OutputType = EArchiverOutput.StaticLibrary;
        }
    }
}

    [Bam.Core.V2.SettingsExtensions(typeof(C.V2.DefaultSettings.DefaultSettingsExtensions))]
    public interface ICommonArchiverOptions : Bam.Core.V2.ISettingsBase
    {
        C.EArchiverOutput OutputType
        {
            get;
            set;
        }
    }
}
    public interface IArchiverOptions
    {
        /// <summary>
        /// The output type of the archiving operation
        /// </summary>
        C.EArchiverOutput OutputType
        {
            get;
            set;
        }

        /// <summary>
        /// Additional options passed to the archiver
        /// </summary>
        string AdditionalOptions
        {
            get;
            set;
        }
    }
}
