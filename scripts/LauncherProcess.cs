using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace IndicaAI.Launcher
{
    // A private, non-inherited job handle ties only this server to its launcher host.
    public sealed class ServerProcess : IDisposable
    {
        private IntPtr job;
        private IntPtr process;
        private ServerProcess(IntPtr job, IntPtr process) { this.job = job; this.process = process; }

        public static ServerProcess Start(string executable, string workingDirectory)
        {
            IntPtr job = CreateJobObject(IntPtr.Zero, null);
            if (job == IntPtr.Zero) throw new Win32Exception(Marshal.GetLastWin32Error());
            PROCESS_INFORMATION child = new PROCESS_INFORMATION();
            try
            {
                JOBOBJECT_EXTENDED_LIMIT_INFORMATION limits = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION();
                limits.BasicLimitInformation.LimitFlags = 0x2000; // JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE
                if (!SetInformationJobObject(job, 9, ref limits, (uint)Marshal.SizeOf(limits)))
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                STARTUPINFO startup = new STARTUPINFO();
                startup.cb = (uint)Marshal.SizeOf(startup);
                // Suspend before running any application code, then attach to the lifetime job.
                if (!CreateProcess(executable, new StringBuilder("\"" + executable + "\""), IntPtr.Zero,
                    IntPtr.Zero, false, 4, IntPtr.Zero, workingDirectory, ref startup, out child))
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                if (!AssignProcessToJobObject(job, child.hProcess))
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                if (ResumeThread(child.hThread) == UInt32.MaxValue)
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                return new ServerProcess(job, child.hProcess);
            }
            catch
            {
                if (child.hProcess != IntPtr.Zero) { TerminateProcess(child.hProcess, 1); CloseHandle(child.hProcess); }
                CloseHandle(job);
                throw;
            }
            finally { if (child.hThread != IntPtr.Zero) CloseHandle(child.hThread); }
        }

        public bool WaitForExit(int milliseconds)
        {
            if (process == IntPtr.Zero) throw new ObjectDisposedException("ServerProcess");
            uint result = WaitForSingleObject(process, (uint)milliseconds);
            if (result == UInt32.MaxValue) throw new Win32Exception(Marshal.GetLastWin32Error());
            return result == 0;
        }
        public int ExitCode
        {
            get { uint result; if (!GetExitCodeProcess(process, out result)) throw new Win32Exception(Marshal.GetLastWin32Error()); return unchecked((int)result); }
        }
        public void Dispose()
        {
            if (job != IntPtr.Zero) { CloseHandle(job); job = IntPtr.Zero; }
            if (process != IntPtr.Zero) { CloseHandle(process); process = IntPtr.Zero; }
            GC.SuppressFinalize(this);
        }
        ~ServerProcess() { Dispose(); }

        [StructLayout(LayoutKind.Sequential)] private struct BASIC_LIMIT_INFORMATION
        { public long PerProcessUserTimeLimit, PerJobUserTimeLimit; public uint LimitFlags; public UIntPtr MinimumWorkingSetSize, MaximumWorkingSetSize; public uint ActiveProcessLimit; public UIntPtr Affinity; public uint PriorityClass, SchedulingClass; }
        [StructLayout(LayoutKind.Sequential)] private struct IO_COUNTERS
        { public ulong ReadOperationCount, WriteOperationCount, OtherOperationCount, ReadTransferCount, WriteTransferCount, OtherTransferCount; }
        [StructLayout(LayoutKind.Sequential)] private struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
        { public BASIC_LIMIT_INFORMATION BasicLimitInformation; public IO_COUNTERS IoInfo; public UIntPtr ProcessMemoryLimit, JobMemoryLimit, PeakProcessMemoryUsed, PeakJobMemoryUsed; }
        [StructLayout(LayoutKind.Sequential)] private struct STARTUPINFO
        { public uint cb; public IntPtr lpReserved, lpDesktop, lpTitle; public uint dwX, dwY, dwXSize, dwYSize, dwXCountChars, dwYCountChars, dwFillAttribute, dwFlags; public ushort wShowWindow, cbReserved2; public IntPtr lpReserved2, hStdInput, hStdOutput, hStdError; }
        [StructLayout(LayoutKind.Sequential)] private struct PROCESS_INFORMATION
        { public IntPtr hProcess, hThread; public uint dwProcessId, dwThreadId; }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)] private static extern IntPtr CreateJobObject(IntPtr attributes, string name);
        [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool SetInformationJobObject(IntPtr job, int infoClass, ref JOBOBJECT_EXTENDED_LIMIT_INFORMATION info, uint length);
        [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool AssignProcessToJobObject(IntPtr job, IntPtr process);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool CreateProcess(string application, StringBuilder command, IntPtr processAttributes, IntPtr threadAttributes, bool inheritHandles, uint flags, IntPtr environment, string directory, ref STARTUPINFO startup, out PROCESS_INFORMATION process);
        [DllImport("kernel32.dll", SetLastError = true)] private static extern uint ResumeThread(IntPtr thread);
        [DllImport("kernel32.dll", SetLastError = true)] private static extern uint WaitForSingleObject(IntPtr handle, uint milliseconds);
        [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool GetExitCodeProcess(IntPtr process, out uint exitCode);
        [DllImport("kernel32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool TerminateProcess(IntPtr process, uint exitCode);
        [DllImport("kernel32.dll")] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool CloseHandle(IntPtr handle);
    }
}
