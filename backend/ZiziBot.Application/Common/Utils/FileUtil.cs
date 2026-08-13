using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text;

namespace ZiziBot.Application.Common.Utils;

public static class FileUtil
{
    private static readonly ConcurrentDictionary<string, ReaderWriterLockSlim> FileLocks = new();
    private static readonly TimeSpan LockTimeout = TimeSpan.FromSeconds(30);
    private static readonly object CleanupLock = new();
    private static long _lastCleanupTicks = DateTime.UtcNow.Ticks;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string NormalizePath(string path) => Path.GetFullPath(path);

    private static ReaderWriterLockSlim GetFileLock(string filePath)
    {
        var normalizedPath = NormalizePath(filePath);
        CleanupOldLocksIfNeeded();
        return FileLocks.GetOrAdd(normalizedPath, static _ => new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion));
    }

    private static void CleanupOldLocksIfNeeded()
    {
        var currentTicks = DateTime.UtcNow.Ticks;
        var lastCleanup = Interlocked.Read(ref _lastCleanupTicks);

        if (currentTicks - lastCleanup < TimeSpan.FromMinutes(5).Ticks)
            return;

        if (!Monitor.TryEnter(CleanupLock))
            return;

        try
        {
            lastCleanup = Interlocked.Read(ref _lastCleanupTicks);
            if (currentTicks - lastCleanup < TimeSpan.FromMinutes(5).Ticks)
                return;

            var locksToRemove = new List<string>();

            foreach (var (key, lockSlim) in FileLocks)
            {
                if (!lockSlim.IsReadLockHeld && !lockSlim.IsWriteLockHeld && !lockSlim.IsUpgradeableReadLockHeld)
                    locksToRemove.Add(key);
            }

            foreach (var key in locksToRemove)
            {
                if (FileLocks.TryRemove(key, out var lockToDispose))
                {
                    lockToDispose.Dispose();
                }
            }

            Interlocked.Exchange(ref _lastCleanupTicks, currentTicks);
        }
        finally
        {
            Monitor.Exit(CleanupLock);
        }
    }

    public static async Task WriteAllTextThreadSafeAsync(this string path, string contents, Encoding? encoding = null, CancellationToken cancellationToken = default)
    {
        encoding ??= Encoding.UTF8;
        var normalizedPath = NormalizePath(path);
        var fileLock = GetFileLock(normalizedPath);
        var writeLockAcquired = false;

        try
        {
            writeLockAcquired = fileLock.TryEnterWriteLock(LockTimeout);
            if (!writeLockAcquired)
                throw new TimeoutException($"Failed to acquire write lock for file: {normalizedPath}");

            var directory = Path.GetDirectoryName(normalizedPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            var tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            try
            {
                await File.WriteAllTextAsync(tempFile, contents, encoding, cancellationToken);
                File.Move(tempFile, normalizedPath, overwrite: true);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }
        finally
        {
            if (writeLockAcquired && fileLock.IsWriteLockHeld)
                fileLock.ExitWriteLock();
        }
    }

    public static async Task<string> ReadAllTextThreadSafeAsync(this string path, Encoding? encoding = null, CancellationToken cancellationToken = default)
    {
        encoding ??= Encoding.UTF8;
        var normalizedPath = NormalizePath(path);
        var fileLock = GetFileLock(normalizedPath);
        var lockAcquired = false;

        try
        {
            lockAcquired = fileLock.TryEnterReadLock(LockTimeout);
            if (!lockAcquired)
                throw new TimeoutException($"Failed to acquire read lock for file: {normalizedPath}");

            return await File.ReadAllTextAsync(normalizedPath, encoding, cancellationToken);
        }
        finally
        {
            if (lockAcquired && fileLock.IsReadLockHeld)
                fileLock.ExitReadLock();
        }
    }

    public static void WriteAllTextThreadSafe(this string path, string contents, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;
        var normalizedPath = NormalizePath(path);
        var fileLock = GetFileLock(normalizedPath);
        var writeLockAcquired = false;

        try
        {
            writeLockAcquired = fileLock.TryEnterWriteLock(LockTimeout);
            if (!writeLockAcquired)
                throw new TimeoutException($"Failed to acquire write lock for file: {normalizedPath}");

            var directory = Path.GetDirectoryName(normalizedPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            var tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            try
            {
                File.WriteAllText(tempFile, contents, encoding);
                File.Move(tempFile, normalizedPath, overwrite: true);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }
        finally
        {
            if (writeLockAcquired && fileLock.IsWriteLockHeld)
                fileLock.ExitWriteLock();
        }
    }

    public static string ReadAllTextThreadSafe(this string path, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;
        var normalizedPath = NormalizePath(path);
        var fileLock = GetFileLock(normalizedPath);
        var lockAcquired = false;

        try
        {
            lockAcquired = fileLock.TryEnterReadLock(LockTimeout);
            if (!lockAcquired)
                throw new TimeoutException($"Failed to acquire read lock for file: {normalizedPath}");

            return File.ReadAllText(normalizedPath, encoding);
        }
        finally
        {
            if (lockAcquired && fileLock.IsReadLockHeld)
                fileLock.ExitReadLock();
        }
    }

    public static void DeleteThreadSafe(this string path, bool throwOnError = false)
    {
        var normalizedPath = NormalizePath(path);
        var fileLock = GetFileLock(normalizedPath);
        var writeLockAcquired = false;

        try
        {
            if (!File.Exists(normalizedPath))
                return;

            writeLockAcquired = fileLock.TryEnterWriteLock(LockTimeout);

            if (!writeLockAcquired)
            {
                if (throwOnError)
                    throw new TimeoutException($"Failed to acquire write lock for file: {normalizedPath}");

                return;
            }

            File.Delete(normalizedPath);
        }
        catch when (!throwOnError)
        {
            // Ignore errors if throwOnError is false
        }
        finally
        {
            if (writeLockAcquired && fileLock.IsWriteLockHeld)
                fileLock.ExitWriteLock();
        }
    }

    public static async Task DeleteThreadSafeAsync(this string path, bool throwOnError = false, CancellationToken cancellationToken = default)
    {
        var normalizedPath = NormalizePath(path);
        var fileLock = GetFileLock(normalizedPath);
        var writeLockAcquired = false;

        try
        {
            if (!File.Exists(normalizedPath))
                return;

            writeLockAcquired = fileLock.TryEnterWriteLock(LockTimeout);

            if (!writeLockAcquired)
            {
                if (throwOnError)
                    throw new TimeoutException($"Failed to acquire write lock for file: {normalizedPath}");

                return;
            }

            await Task.Run(() => File.Delete(normalizedPath), cancellationToken);
        }
        catch when (!throwOnError)
        {
            // Ignore errors if throwOnError is false
        }
        finally
        {
            if (writeLockAcquired && fileLock.IsWriteLockHeld)
                fileLock.ExitWriteLock();
        }
    }
}