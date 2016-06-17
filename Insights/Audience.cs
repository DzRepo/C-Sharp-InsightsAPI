using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
// ReSharper disable InconsistentNaming

namespace Insights
{
    // ReSharper disable once InconsistentNaming        
    public class AudienceAPI
    {
        #region Method Return Classes
        public class Segment
        {
            public string id { get; set; }
            public DateTime created { get; set; }
            public DateTime last_modified { get; set; }
            public string name { get; set; }
            public string num_user_ids { get; set; }
            public string num_distinct_user_ids { get; set; }
            public string state { get; set; }
            public object[] audience_ids { get; set; }
            public bool ErrorFlag { get; set; }
            public Exception Error { get; set; }
            public string ErrorMessage { get; set; }
        }

        public class Segments
        {
            public Segment[] segments { get; set; }
            public string next { get; set; }
            public bool ErrorFlag { get; set; }
            public Exception Error { get; set; }
            public string ErrorMessage { get; set; }
        }
        public class Audience
        {
            public string id { get; set; }
            public string name { get; set; }
            public DateTime created { get; set; }
            public DateTime last_modified { get; set; }
            public string num_user_ids { get; set; }
            public string num_distinct_user_ids { get; set; }
            public string state { get; set; }
            public string[] segment_ids { get; set; }
            public bool ErrorFlag { get; set; }
            public Exception Error { get; set; }
            public string ErrorMessage { get; set; }
        }

        public class Audiences
        {
            public Audience[] audiences { get; set; }
            public string next { get; set; }
            public bool ErrorFlag { get; set; }
            public Exception Error { get; set; }
            public string ErrorMessage { get; set; }
        }

        public class QueryResponse
        {
            public string Text { get; set; }
            public bool ErrorFlag { get; set; }
            public Exception Error { get; set; }
            public string ErrorMessage { get; set; }
        }
        public class Usage
        {
            public string segments_limit_limit { get; set; }
            public string segments_limit_remaining { get; set; }
            public string audiences_limit_limit { get; set; }
            public string audiences_limit_remaining { get; set; }
            public string monthly_queries_rate_limit_limit { get; set; }
            public string monthly_queries_rate_limit_remaining { get; set; }
            public string monthly_queries_rate_limit_reset { get; set; }
            public bool ErrorFlag { get; set; }
            public Exception Error { get; set; }
            public string ErrorMessage { get; set; }
        }

        #endregion
        private readonly OAuthManager _manager;
        #region Constructor
        public AudienceAPI(string consumerKey, string consumerSecret, string token = null, string tokenSecret = null)
        {
            _manager = new OAuthManager(consumerKey, consumerSecret, token, tokenSecret);
        }
        #endregion
        public Segment CreateSegment(string name)
        {
            Segment segment = new Segment();
            JObject request = new JObject();
            request["name"] = name;
            OAuthResponse response = null;
            try
            {
                response = _manager.GetOAuthResponse("POST", "audience/segments", request.ToString());
                if (response.ErrorFlag) throw response.Error;
                else
                {
                    Segment s = JsonConvert.DeserializeObject<Segment>(response.ResponseString);
                    segment = s;
                }
            }
            catch (Exception ex)
            {
                segment.ErrorFlag = true;
                segment.Error = ex;
                if (response != null) segment.ErrorMessage = response.ResponseString;
            }
            return segment;

        }
        public Segments GetSegments()
        {
            OAuthResponse response = null;
            List<Segment> segments = new List<Segment>();
            Segments segmentsResponse = new Segments();
            try
            {
                response = _manager.GetOAuthResponse("GET", "audience/segments");
                if (response.ErrorFlag) throw response.Error;
                else
                {
                    var repeatFlag = true;
                    while (repeatFlag)
                    {
                        Segments s = JsonConvert.DeserializeObject<Segments>(response.ResponseString);
                        repeatFlag = (s.next != null);
                        segments.AddRange(s.segments);
                    }
                }
                segmentsResponse.segments = segments.ToArray();
            }
            catch (Exception ex)
            {
                segmentsResponse.ErrorFlag = true;
                segmentsResponse.Error = ex;
                if (response != null) segmentsResponse.ErrorMessage = response.ResponseString;
            }
            return segmentsResponse;

        }
        public Segment GetSegment(string id)
        {
            Segment segment = new Segment();
            OAuthResponse response = null;
            try
            {
                response = _manager.GetOAuthResponse("GET", "audience/segments/" + id);
                if (response.ErrorFlag) throw response.Error;
                else
                {
                    Segment s = JsonConvert.DeserializeObject<Segment>(response.ResponseString);
                    segment = s;
                }
            }
            catch (Exception ex)
            {
                segment.ErrorFlag = true;
                segment.Error = ex;
                if (response != null) segment.ErrorMessage = response.ResponseString;
            }
            return segment;

        }
        public Segment AppendToSegment(string segmentId, string[] actorIds)
        {
            Segment segment = new Segment();
            JObject request = new JObject();
            request["user_ids"] = new JArray { actorIds };

            OAuthResponse response = null;
 
            try
            {
                response = _manager.GetOAuthResponse("POST", "audience/segments/" + segmentId + "/ids", request.ToString());
                if (response.ErrorFlag) throw response.Error;
                else
                {
                    Segment s = JsonConvert.DeserializeObject<Segment>(response.ResponseString);
                    segment = s;
                }
            }
            catch (Exception ex)
            {
                segment.ErrorFlag = true;
                segment.Error = ex;
                if (response != null) segment.ErrorMessage = response.ResponseString;
            }
            return segment;
        }
        public Segment DeleteSegment(string segmentId)
        {
            Segment segment = new Segment();
            OAuthResponse response = null;
            try
            {
                response = _manager.GetOAuthResponse("DELETE", "audience/segments/" + segmentId);
                if (response.ErrorFlag) throw response.Error;
                else
                {
                    segment.id = segmentId;
                    segment.state = "Deleted";
                    segment.last_modified = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                segment.ErrorFlag = true;
                segment.Error = ex;
                if (response != null) segment.ErrorMessage = response.ResponseString;
            }
            return segment;
        }
        public Audience CreateAudience(string name, string[] segmentIds)
        {
            JObject audienceCreate = new JObject();
            audienceCreate["name"] = name;
            audienceCreate["segment_ids"] =  new JArray {segmentIds};
            var audience = new Audience();

            OAuthResponse response = null;
            try
            {
                response = _manager.GetOAuthResponse("POST", "audience/audiences", audienceCreate.ToString());
                if (response.ErrorFlag) throw response.Error;
                else
                {
                    Audience a = JsonConvert.DeserializeObject<Audience>(response.ResponseString);
                    audience = a;
                }
            }
            catch (Exception ex)
            {
                audience.ErrorFlag = true;
                audience.Error = ex;
                if (response != null) audience.ErrorMessage = response.ResponseString;
            }
            return audience;
        }
        public Audiences GetAudiences()
        {
            OAuthResponse response = null;
            List<Audience> audiences = new List<Audience>();
            Audiences audiencesResponse = new Audiences();
            try
            {
                response = _manager.GetOAuthResponse("GET", "audience/audiences");
                if (response.ErrorFlag) throw response.Error;
                else
                {
                    var repeatFlag = true;
                    while (repeatFlag)
                    {
                        Audiences s = JsonConvert.DeserializeObject<Audiences>(response.ResponseString);
                        repeatFlag = (s.next != null);
                        audiences.AddRange(s.audiences);
                    }
                }
                audiencesResponse.audiences = audiences.ToArray();
            }
            catch (Exception ex)
            {
                audiencesResponse.ErrorFlag = true;
                audiencesResponse.Error = ex;
                if (response != null) audiencesResponse.ErrorMessage = response.ResponseString;
            }
            return audiencesResponse;

        }
        public Audience GetAudience(string id)
        {
            Audience audience = new Audience();
            OAuthResponse response = null;
            try
            {
                response = _manager.GetOAuthResponse("GET", "audience/audiences/" + id);
                if (response.ErrorFlag) throw response.Error;
                else
                {
                    Audience s = JsonConvert.DeserializeObject<Audience>(response.ResponseString);
                    audience = s;
                }
            }
            catch (Exception ex)
            {
                audience.ErrorFlag = true;
                audience.Error = ex;
                if (response != null) audience.ErrorMessage = response.ResponseString;
            }
            return audience;

        }
        public QueryResponse QueryAudience(string audienceId, string groupings)
        {
            var qResponse = new QueryResponse();
            OAuthResponse response = null;
            try
            {
                response = _manager.GetOAuthResponse("POST", "audience/audiences/" + audienceId + "/query" , groupings);
                if (response.ErrorFlag) throw response.Error;
                else
                    qResponse.Text = response.ResponseString;
            }
            catch (Exception ex)
            {
                qResponse.ErrorFlag = true;
                qResponse.Error = ex;
                if (response != null) qResponse.ErrorMessage = response.ResponseString;
            }
            return qResponse;
        }
        public Audience DeleteAudience(string audienceId)
        {
            Audience audience = new Audience();
            OAuthResponse response = null;
            try
            {
                response = _manager.GetOAuthResponse("DELETE", "audience/audiences/" + audienceId);
                if (response.ErrorFlag) throw response.Error;
                else
                {
                    audience.id = audienceId;
                    audience.state = "Deleted";
                    audience.last_modified = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                audience.ErrorFlag = true;
                audience.Error = ex;
                if (response != null) audience.ErrorMessage = response.ResponseString;
            }
            return audience;
        }

        public Usage GetUsage()
        {
            OAuthResponse response = null;
            var usage = new Usage();
            try
            {
                response = _manager.GetOAuthResponse("GET", "audience/usage");
                if (response.ErrorFlag) throw response.Error;
                else
                {
                    var u = JsonConvert.DeserializeObject<Usage>(response.ResponseString);
                    usage = u;
                }
            }
            catch (Exception ex)
            {
                usage.ErrorFlag = true;
                usage.Error = ex;
                if (response != null) usage.ErrorMessage = response.ResponseString;
            }
            return usage;
            
        }
    }
}
