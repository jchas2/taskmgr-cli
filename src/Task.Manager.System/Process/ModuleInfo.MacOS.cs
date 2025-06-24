using System.Runtime.InteropServices;
using Task.Manager.Interop.Mach;
using SysDiag = System.Diagnostics;

namespace Task.Manager.System.Process;

public partial class ModuleInfo
{
#if __APPLE__
    private static bool GetModulesInternal(SysDiag::Process process, out List<ModuleInfo> moduleInfos)
    {
        moduleInfos = new List<ModuleInfo>();
        uint imageCount = DyLib._dyld_image_count();

        for (uint i = 0; i < imageCount; i++) {
            IntPtr imagePathPtr = DyLib._dyld_get_image_name(i);

            if (imagePathPtr == IntPtr.Zero) {
                continue;
            }

            string? imagePath = Marshal.PtrToStringAnsi(imagePathPtr);

            if (string.IsNullOrWhiteSpace(imagePath)) {
                continue;
            }

            var moduleInfo = new ModuleInfo() {
                ModuleName = Path.GetFileName(imagePath),
                FileName = imagePath
            };
            
            moduleInfos.Add(moduleInfo);
        }

        return moduleInfos.Count > 0;
    }
#endif
}