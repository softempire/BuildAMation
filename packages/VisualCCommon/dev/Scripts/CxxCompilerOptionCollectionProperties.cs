// Automatically generated file from OpusOptionInterfacePropertyGenerator. DO NOT EDIT.
// Command line:
// -i=..\..\..\C\dev\Scripts\ICxxCompilerOptions.cs -n=VisualCCommon -c=CxxCompilerOptionCollection -p -d -dd=..\..\..\CommandLineProcessor\dev\Scripts\CommandLineDelegate.cs;..\..\..\VisualStudioProcessor\dev\Scripts\VisualStudioDelegate.cs -pv=PrivateData -e
namespace VisualCCommon
{
    public partial class CxxCompilerOptionCollection
    {
        #region C.ICxxCompilerOptions Option properties
        C.Cxx.EExceptionHandler C.ICxxCompilerOptions.ExceptionHandler
        {
            get
            {
                return this.GetValueTypeOption<C.Cxx.EExceptionHandler>("ExceptionHandler");
            }
            set
            {
                this.SetValueTypeOption<C.Cxx.EExceptionHandler>("ExceptionHandler", value);
                this.ProcessNamedSetHandler("ExceptionHandlerSetHandler", this["ExceptionHandler"]);
            }
        }
        #endregion
    }
}
