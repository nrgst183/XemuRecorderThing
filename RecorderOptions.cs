using ScreenRecorderLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RecorderThing
{
    public static class DefaultRecorderOptions
    {
        public static RecorderOptions GetDefaultRecorderOptionsFromXemuProcess(Process xemuProcess)
        {
            return new RecorderOptions
            {
                AudioOptions = new AudioOptions
                {
                    AudioInputDevice = null,
                    AudioOutputDevice = null,
                    IsAudioEnabled = true,
                    IsInputDeviceEnabled = false,
                    IsOutputDeviceEnabled = true,
                    OutputVolume = (float?)0.8
                },
                VideoEncoderOptions = new VideoEncoderOptions
                {
                    Bitrate = 8000 * 1000,
                    Framerate = 60,
                    IsFixedFramerate = true,
                    //Currently supported are H264VideoEncoder and H265VideoEncoder
                    Encoder = new H264VideoEncoder
                    {
                        BitrateMode = H264BitrateControlMode.CBR,
                        EncoderProfile = H264Profile.Main,
                    },
                    //Fragmented Mp4 allows playback to start at arbitrary positions inside a video stream,
                    //instead of requiring to read the headers at the start of the stream.
                    IsFragmentedMp4Enabled = true,
                    //If throttling is disabled, out of memory exceptions may eventually crash the program,
                    //depending on encoder settings and system specifications.
                    IsThrottlingDisabled = false,
                    //Hardware encoding is enabled by default.
                    IsHardwareEncodingEnabled = true,
                    //Low latency mode provides faster encoding, but can reduce quality.
                    IsLowLatencyEnabled = true,
                    //Fast start writes the mp4 header at the beginning of the file, to facilitate streaming.
                    IsMp4FastStartEnabled = false
                },
                SourceOptions = new SourceOptions
                {
                    RecordingSources = new List<RecordingSourceBase>
                    {
                        new WindowRecordingSource(xemuProcess.MainWindowHandle)
                    }
                }
            };
        }
    }
}
