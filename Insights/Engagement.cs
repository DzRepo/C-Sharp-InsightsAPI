using System;
using Newtonsoft.Json.Linq;

namespace Insights
{
    public class EngagementAPI
    {
        private const string Engagement28HrHistorical = "{ \"engagement_types\": [\"impressions\", \"engagements\", \"favorites\", \"replies\", \"retweets\", \"url_clicks\", \"hashtag_clicks\", \"media_clicks\", \"user_follows\",\"user_profile_clicks\"]}";
        private const string EngagementTotals = "{\"engagement_types\": [\"retweets\", \"favorites\", \"replies\"]}";

        private const string DefaultGrouping = "{\"by-tweet-id\": { \"group_by\": [\"tweet.id\",\"engagement.type\"]}}";

        private readonly OAuthManager _manager;

        public EngagementAPI(string consumerKey, string consumerSecret, string token = null, string tokenSecret = null)
        {
            _manager = new OAuthManager(consumerKey, consumerSecret, token, tokenSecret);
        }

        public OAuthResponse Get28Hr(string[] tweetIds, string groupings = null)
        {
            JObject request = JObject.Parse(Engagement28HrHistorical);

            request["tweet_ids"] = new JArray {tweetIds};
 
            if (groupings == null)
                request["groupings"] = JObject.Parse(DefaultGrouping);
            else
                request["groupings"] = JObject.Parse(groupings);

            var response = _manager.GetOAuthResponse("POST", "engagement/28hr", request.ToString());
            return response;
        }

        public OAuthResponse GetHistorical(string[] tweetIds, DateTime? fromDate = null, DateTime? toDate = null,
            string groupings = null)
        {
            JObject request = JObject.Parse(Engagement28HrHistorical);

            request["tweet_ids"] = new JArray {tweetIds};
 
            if (groupings == null)
                request["groupings"] = JObject.Parse(DefaultGrouping);
            else
                request["groupings"] = JObject.Parse(groupings);

            if (fromDate != null)
                request["start"] = DateTime.Parse(fromDate.ToString()).ToString("yyyy-MM-dd");

            if (toDate != null)
                request["end"] = DateTime.Parse(toDate.ToString()).ToString("yyyy-MM-dd");

            var response = _manager.GetOAuthResponse("POST", "engagement/historical", request.ToString());
            return response;
        }

        public OAuthResponse GetTotals(string[] tweetIds, string groupings = null)
        {
            JObject request = JObject.Parse(EngagementTotals);

            request["tweet_ids"] = new JArray {tweetIds};
            // request["engagement_types"] = new JArray("impressions", "retweets", "favorites");

            if (groupings == null)
                request["groupings"] = JObject.Parse(DefaultGrouping);
            else
                request["groupings"] = JObject.Parse(groupings);

            var response = _manager.GetOAuthResponse("POST", "engagement/totals", request.ToString());
            return response;
        }

    }
}
