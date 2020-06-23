namespace HomeSecureApi
{
    public class HsConfig
    {
        public string ClientToken{get;set;}

        public double ClientSessionTTLSeconds{get;set;}=60;

        public string HlsCommand{get;set;}=
            "ffmpeg -i {INPUT} -c:v copy -preset ultrafast -fflags flush_packets "+
            "-tune zerolatency -an -g 1 -flags -global_header "+
            "-f hls -hls_time 1 -hls_list_size 15 -hls_wrap 15 {STREAM_NAME}";

        public string HlsRoot{get;set;}="../../streams";

        public string StreamsConfig{get;set;}="../../streams.json";
    }
}