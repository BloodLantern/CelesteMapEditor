#include "logger.hpp"

#include <iostream>

#include <windows.h>

#define ANSI_COLOR_YELLOW   "\x1b[33m"
#define ANSI_COLOR_RED      "\x1b[31m"

#define ANSI_STYLE_BOLD     "\x1b[1m"

#define ANSI_RESET          "\x1b[0m"

TsQueue<editor::Logger::LogEntry> editor::Logger::mLines;
std::ofstream editor::Logger::mFile;
std::condition_variable editor::Logger::mCondVar;

std::thread editor::Logger::thread = std::thread(&editor::Logger::Run);
std::mutex mutex;
bool synchronizing = false;
bool running = true;

void editor::Logger::OpenFile(const std::filesystem::path &filename)
{
    CloseFile();
    
    const bool exists = std::filesystem::exists(filename);
    if (!exists)
        std::filesystem::create_directories(filename.parent_path());

    mFile.open(filename, std::ios_base::out | std::ios_base::app);

    if (!mFile.is_open() || !mFile.good())
    {
        LogWarning("Could not open log file for writing: " + std::filesystem::absolute(filename).string());
        return;
    }

    LogInfo("Logging to file: %s", filename.string().c_str());

    // If the file already exists, add newlines to space from the last log
    if (!exists)
        LogInfo("Starting logging #0");
    else
    {
        // Write a newline to separate each log entry and use std::endl to make
        // sure to flush it so that when we count the number of newlines, we get
        // the correct number
        mFile << std::endl;

        // Read file contents to count empty lines and therefore know how many logs
        // where written in the file.
        std::ifstream in(filename);

        if (!in.is_open() || !in.good())
            LogWarning("Could not open log file for reading: " + std::filesystem::absolute(filename).string());
        else
        {
            std::string line;
            unsigned int count = 0;
            while (!in.eof())
            {
                std::getline(in, line);
                if (line.empty() || line == "\n")
                    count++;
            }
            LogInfo("Starting logging #%d", count - 1);
        }
    }
}

void editor::Logger::OpenDefaultFile()
{
    // Get the current date and format it in yyyy-mm-dd for the file name
    const std::time_t t = std::chrono::system_clock::to_time_t(std::chrono::system_clock::now());
    std::tm ltime;
    localtime_s(&ltime, &t);
    const std::_Timeobj<char, const tm *> timeFormatter = std::put_time(&ltime, "%F.log");
    const std::string date = (std::ostringstream() << timeFormatter).str();
    OpenFile("logs/" + date);
}

void editor::Logger::CloseFile()
{
    if (mFile.is_open())
    {
        mFile.flush();
        mFile.close();
    }
}

void editor::Logger::Synchronize()
{
    if (!mLines.Empty())
    {
        synchronizing = true;
        mCondVar.notify_one();
        std::unique_lock<std::mutex> lock(mutex);
        mCondVar.wait(lock, [] { return !synchronizing; });
    }
}

void editor::Logger::Stop()
{
    editor::Logger::Synchronize();
    running = false;
    mCondVar.notify_one();
    if (thread.joinable())
        thread.join();
}

void editor::Logger::Run()
{
    // Set thread name for easier debugging
    SetThreadDescription(thread.native_handle(), L"Logger Thread");
    std::unique_lock<std::mutex> lock(mutex);
    while (running)
    {
        mCondVar.wait(lock, [] { return !mLines.Empty() || !running || synchronizing; });

        while (!mLines.Empty())
            Log(mLines.Pop());

        // As we don't use std::endl for newlines, make sure to flush the streams before going to sleep
        std::cout.flush();
        if (mFile.is_open())
            mFile.flush();

        if (synchronizing)
        {
            synchronizing = false;
            mCondVar.notify_one();
        }
    }
    CloseFile();
}

void editor::Logger::Log(const LogEntry& entry)
{
    // Get the message time and format it in [hh:mm:ss]
    const std::time_t t = std::chrono::system_clock::to_time_t(entry.time);
    std::tm ltime;
    localtime_s(&ltime, &t);
    const std::_Timeobj<char, const tm *> timeFormatter = std::put_time(&ltime, "[%H:%M:%S] ");
    const std::string time = (std::ostringstream() << timeFormatter).str();

    // Setup the base text message
    std::string baseMessage = entry.message + '\n';
    LogLevel level = entry.level;
    
    const bool outputToVS = (int) level & VS_OUTPUT_LOG_BIT;
    if (outputToVS)
        level = LOG_LEVEL_BINARY_OP(level, ~VS_OUTPUT_LOG_BIT, &);

    const char* color = ANSI_RESET;
    switch (level)
    {
        case LogLevel::Info:
            baseMessage = time + "[INFO] " + baseMessage;
            break;
        case LogLevel::Warning:
            color = ANSI_COLOR_YELLOW;
            baseMessage = time + "[WARN] " + baseMessage;
            break;
        case LogLevel::Error:
            color = ANSI_COLOR_RED;
            baseMessage = time + "[ERROR] " + baseMessage;
            break;
        case LogLevel::Fatal:
            color = ANSI_STYLE_BOLD ANSI_COLOR_RED;
            baseMessage = time + "[FATAL] " + baseMessage;
            break;
    }

    std::cout << color + baseMessage + ANSI_RESET;

    if (mFile.is_open())
        mFile << baseMessage;

    if (outputToVS)
        OutputDebugStringA(baseMessage.c_str());
}
