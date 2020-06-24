namespace HomeSecureApi
{
    public class HsConfig
    {

        public string LocationId{get;set;}

        public string ClientToken{get;set;}

        public double ClientSessionTTLSeconds{get;set;}=60;

        public string HlsCommand{get;set;}=
            "ffmpeg -i {INPUT} -c:v copy -preset ultrafast -fflags flush_packets "+
            "-tune zerolatency -an -g 1 -flags -global_header "+
            "-f hls -hls_time 1 -hls_list_size 15 -hls_wrap 15 {STREAM_NAME}";

        public string HlsRoot{get;set;}="../../streams";

        public string StreamsConfig{get;set;}="../../streams.json";

        public double StreamStartTimeoutSeconds{get;set;}=15;


        public int SmtpPort{get;set;}=25;

        public string SmtpListenAddress{get;set;}="0.0.0.0";

        public bool SmtpDebug{get;set;}
    }
}