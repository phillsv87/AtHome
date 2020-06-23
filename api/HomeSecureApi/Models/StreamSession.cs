using System;

namespace HomeSecureApi.Models
{
    public class StreamSession
    {

        public Guid Id{get;set;}

        public int StreamId{get;set;}

        public string StreamName{get;set;}

        public string Token{get;set;}

        public DateTime Expirers{get;set;}

        public double TTLSeconds{get;set;}

        public string Uri{get;set;}
    }
}