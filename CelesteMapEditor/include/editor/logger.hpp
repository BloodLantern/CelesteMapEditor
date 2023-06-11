#pragma once

#include <filesystem>
#include <fstream>
#include <thread>
#include <condition_variable>
#include <utility>

#include "tsqueue.hpp"

#define STRINGIFY(x) #x
#define TOSTRING(x) STRINGIFY(x)
#define DEBUG_LOG(str, args) Logger::LogInfoToVS(std::filesystem::relative(__FILE__).string() + "(" TOSTRING(__LINE__) "): " str, args)
#define LOG_LEVEL_BINARY_OP(left, right, operator) (static_cast<Logger::LogLevel>(static_cast<unsigned char>(left) operator right))

namespace editor
{
    class Logger
    {
    public:
        // You cannot instantiate this class
        Logger() = delete;

        enum class LogLevel : unsigned char
        {
            Info,
            Warning,
            Error,
            Fatal
        };

        template<class... Args>
        static void LogInfo(const std::string& format, Args&&... args)
        {
            mLines.Push(LogEntry(Format(format, std::forward<Args>(args)...), LogLevel::Info));
            mCondVar.notify_one();
        }
        template<class... Args>
        static void LogWarning(const std::string& format, Args&&... args)
        {
            mLines.Push(LogEntry(Format(format, std::forward<Args>(args)...), LogLevel::Warning));
            mCondVar.notify_one();
        }
        template<class... Args>
        static void LogError(const std::string& format, Args&&... args)
        {
            mLines.Push(LogEntry(Format(format, std::forward<Args>(args)...), LogLevel::Error));
            mCondVar.notify_one();
        }
        template<class... Args>
        static void LogFatal(const std::string& format, Args&&... args)
        {
            mLines.Push(LogEntry(Format(format, std::forward<Args>(args)...), LogLevel::Fatal));
            mCondVar.notify_one();
        }
        
        template<class... Args>
        static void LogInfoToVS(const std::string& format, Args&&... args)
        {
            mLines.Push(LogEntry(Format(format, std::forward<Args>(args)...), LOG_LEVEL_BINARY_OP(LogLevel::Info, VS_OUTPUT_LOG_BIT, | )));
            mCondVar.notify_one();
        }
        template<class... Args>
        static void LogWarningToVS(const std::string& format, Args&&... args)
        {
            mLines.Push(LogEntry(Format(format, std::forward<Args>(args)...), LOG_LEVEL_BINARY_OP(LogLevel::Warning, VS_OUTPUT_LOG_BIT, | )));
            mCondVar.notify_one();
        }
        template<class... Args>
        static void LogErrorToVS(const std::string& format, Args&&... args)
        {
            mLines.Push(LogEntry(Format(format, std::forward<Args>(args)...), LOG_LEVEL_BINARY_OP(LogLevel::Error, VS_OUTPUT_LOG_BIT, | )));
            mCondVar.notify_one();
        }
        template<class... Args>
        static void LogFatalToVS(const std::string& format, Args&&... args)
        {
            mLines.Push(LogEntry(Format(format, std::forward<Args>(args)...), LOG_LEVEL_BINARY_OP(LogLevel::Fatal, VS_OUTPUT_LOG_BIT, | )));
            mCondVar.notify_one();
        }

        static void OpenFile(const std::filesystem::path& filename);
        static void OpenDefaultFile();
        static bool HasFileOpen() { return mFile.is_open(); }
        static void CloseFile();

        static void Synchronize();
        static void Stop();

    private:
        struct LogEntry
        {
            std::string message;
            LogLevel level;
            std::chrono::system_clock::time_point time;

            LogEntry(const std::string& message, LogLevel level, std::chrono::system_clock::time_point time = std::chrono::system_clock::now())
                : message(message), level(level), time(time) {}
        };

        static constexpr const unsigned char VS_OUTPUT_LOG_BIT = 0b10000000;

        static TsQueue<LogEntry> mLines;

        static std::ofstream mFile;
        static std::condition_variable mCondVar;
        static std::thread thread;

        static void Run();
        static void Log(const LogEntry& entry);

        template<class... Args>
        static inline std::string Format(const std::string& format, Args&&... args)
        {
            char buffer[0x800];
            sprintf_s(buffer, sizeof(buffer), format.c_str(), std::forward<Args>(args)...);
            return buffer;
        }
    };
}
