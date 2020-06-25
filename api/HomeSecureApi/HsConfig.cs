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

        /// <summary>
        /// A key that is used to secure a verity of utility endpoints
        /// </summary>
        public string UtilAuthKey{get;set;}

        public string NotificationDeviceDb{get;set;} = "/etc/homesecure/NotificationDevices.json";


        /// <summary>
        /// Domain of the Apple push server. 
        /// dev =  api.development.push.apple.com, production = api.push.apple.com
        /// </summary>
        public string ApnDomain { get; set; } = "api.push.apple.com";

        /// <summary>
        /// A 10-character key identifier (kid) key for the Key used for APN
        /// </summary>
        public string ApnKid { get; set; }

        /// <summary>
        /// The issuer (iss) registered claim key, whose value is your
        /// 10-character Team ID used for APN
        /// </summary>
        public string ApnIss { get; set; }

        /// <summary>
        /// Private key used for APN. ApnSecretPath can be used to store the
        /// private key in a file
        /// </summary>
        public string ApnSecret { get; set; }

        /// <summary>
        /// Path to private key used for APN
        /// </summary>
        public string ApnSecretPath { get; set; } = "/etc/homesecure/HomeSecurePush.p8";

        /// <summary>
        /// Topic used for APN. In most cases it will be the bundle ID of the
        /// receiving app.
        /// </summary>
        public string ApnTopic { get; set; }
    }
}