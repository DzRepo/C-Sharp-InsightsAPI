using System;
using Insights;
// ReSharper disable UnusedVariable
// ReSharper disable HeuristicUnreachableCode
// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace InsightsTest
{
    class Program
    {
        static void Main()
        {
            // These Keys come from https://apps.twitter.com/app/99999999/keys
            // The numberic value ( 999999999 ) in the URL needs to be sent to Gnip to associate with your account
            // in order to authorize you to use the Insights APIs.
            const string consumerKey = "CONSUMER_KEY";
            const string consumerSecret = "CONSUMER_SECRET";

            //  These keys come from either the same page as the above, or from the three-legged-oauth process that
            // allows another account to authorize you to retrieve engagement data on their behalf.
            
            string token = "TOKEN";
            string tokenSecret = "TOKEN_SECRET";

            // To access Totals for unowned accounts, make sure the token/tokenSecret are null.
            // If they are set, Totals will be "locked" to the account associated with those tokens.
            // token = null;
            // tokenSecret = null;

            // Set these flags to control which test(s) is/are executed

            var EngagementTest = true;
            var AudienceTest = true;

            if (EngagementTest)
            {
                var engagement = new EngagementAPI(consumerKey, consumerSecret, token, tokenSecret);
                TestEngagement(engagement);
            }

            if (AudienceTest)
            {
                var audience = new AudienceAPI(consumerKey, consumerSecret, token, tokenSecret);
                TestAudience(audience);
            }
    
            Console.WriteLine("Press <Return> to Exit.");
            Console.ReadLine();
        }
        private static void TestAudience(AudienceAPI audience)
        {
            var _SegmentName = "NewTestSegment";
            var _AudienceName = "NewTestAudience";

            // This is a file of line-delimited NUMERIC user Ids.
            //  It must have more than 500 unique user Ids, and less than 100,000
            // For more than 100,000 user Ids, call the AppendToSegment method multiple times.
            var UserIDFilename = @"Z:\Documents\Scripts\twitter_user_ids.txt";

            const string groupingsWireless =
                "{\"groupings\": {\"By-Country-Network\": {\"group_by\": [\"user.location.country\", \"user.device.network\"]},\"By-Device\":{\"group_by\":[\"user.device.os\"]}}}";
            const string groupingsGender =
                "{\"groupings\": {\"By-Gender\": {\"group_by\": [\"user.gender\"]},\"By-Gender-Interest\":{\"group_by\":[\"user.gender\", \"user.interest\"]}}}";
            const string groupingsTvShows =
                "{\"groupings\": {\"By-TV-Shows\": {\"group_by\": [\"user.tv.show\"]},\"By-Country-TV-Shows\":{\"group_by\":[\"user.location.country\",\"user.tv.show\"]}}}";
            const string  groupingsBasics =
                "{\"groupings\": {\"By-Network\": {\"group_by\": [\"user.device.network\"]},\"By-Gender\":{\"group_by\":[\"user.gender\"]},\"By-Region\":{\"group_by\":[\"user.location.region\"]},\"By-Langage\":{\"group_by\":[\"user.language\"]}}}";

            
            Console.WriteLine("Create Segment");
            var segment = audience.CreateSegment(_SegmentName);
            if (segment.ErrorFlag)
            {
                Console.WriteLine("Error:" + segment.ErrorMessage);
            }
            else
            {
                Console.WriteLine("Segment Created");
                Console.WriteLine("Id:" + segment.id);
                Console.WriteLine("Created:" + segment.created);
                Console.WriteLine("State:" + segment.state);
            }

            
            Console.WriteLine("Get Segments");
            var segments = audience.GetSegments();
            if (segments.ErrorFlag)
            {
                Console.WriteLine("Error:" + segments.ErrorMessage);
            }
            else
            {
                foreach (var seg in segments.segments)
                {
                    Console.WriteLine("Segment:" + seg.name + " Created:" + seg.created + " ids:" + seg.num_user_ids + " state:" + seg.state );
                }
            }

            if (segments.segments != null)
            {
                Console.WriteLine("GetSegment (by ID)");
                var segmentbyId = audience.GetSegment(segments.segments[0].id);
                if (segmentbyId.ErrorFlag)
                {
                    Console.WriteLine("Error:" + segmentbyId.ErrorMessage);
                }
                else
                {
                    Console.WriteLine("Name:" + segmentbyId.name);
                    Console.WriteLine("Id:" + segmentbyId.id);
                    Console.WriteLine("Created:" + segmentbyId.created);
                    Console.WriteLine("State:" + segmentbyId.state);
                }


                Console.WriteLine("Append Users to Segment");

                string[] ids = System.IO.File.ReadAllLines(UserIDFilename);

                var segmentToAppend = audience.AppendToSegment(segments.segments[0].id, ids);
                Console.WriteLine("Number of IDs to add:" + ids.Length);

                if (segmentToAppend.ErrorFlag)
                {
                    Console.WriteLine("Error:" + segmentToAppend.ErrorMessage);
                }
                else
                {
                    Console.WriteLine("Name:" + segmentToAppend.name);
                    Console.WriteLine("Id:" + segmentToAppend.id);
                    Console.WriteLine("Created:" + segmentToAppend.created);
                    Console.WriteLine("State:" + segmentToAppend.state);
                    Console.WriteLine("Number of IDs:" + segmentToAppend.num_user_ids);
                    Console.WriteLine("Unique IDs:" + segmentToAppend.num_distinct_user_ids);
                }

                Console.WriteLine("Create Audience");
                var createdAudience = audience.CreateAudience(_AudienceName, new[] {segments.segments[0].id});
                if (createdAudience.ErrorFlag)
                    Console.WriteLine("Error:" + createdAudience.ErrorMessage);
                else
                {
                    Console.WriteLine("Id:" + createdAudience.id);
                    Console.WriteLine("Updated:" + createdAudience.last_modified);
                    Console.WriteLine("State:" + createdAudience.state);
                }

                Console.WriteLine("Query Audience");
                var queryResults = audience.QueryAudience(createdAudience.id, groupingsBasics);
                if (queryResults.ErrorFlag)
                    Console.WriteLine("Error:" + queryResults.ErrorMessage);
                else
                {
                    Console.WriteLine(queryResults.Text);
                }

                Console.WriteLine("");
                Console.WriteLine("Delete Audience");
                var audienceToDelete = audience.DeleteAudience(createdAudience.id);
                if (audienceToDelete.ErrorFlag)
                {
                    Console.WriteLine("Error:" + audienceToDelete.ErrorMessage);
                }
                else
                {
                    Console.WriteLine("Id:" + audienceToDelete.id);
                    Console.WriteLine("Updated:" + audienceToDelete.last_modified);
                    Console.WriteLine("State:" + audienceToDelete.state);
                }

                Console.WriteLine("Delete Segment");

                var segmentToDelete = audience.DeleteSegment(segments.segments[0].id);
                if (segmentToDelete.ErrorFlag)
                {
                    Console.WriteLine("Error:" + segmentToDelete.ErrorMessage);
                }
                else
                {
                    Console.WriteLine("Id:" + segmentToDelete.id);
                    Console.WriteLine("Updated:" + segmentToDelete.last_modified);
                    Console.WriteLine("State:" + segmentToDelete.state);
                }
            }

            Console.WriteLine("Audience API Usage");
            var usage = audience.GetUsage();
            if (usage.ErrorFlag)
            {
                Console.WriteLine("Error:" + usage.ErrorMessage);
            }
            else
            {
                Console.WriteLine("Audiences Limit:" + usage.audiences_limit_limit);
                Console.WriteLine("Audiences Remaining:" + usage.audiences_limit_remaining);
                Console.WriteLine("Monthly Queries Rate Limit:" + usage.monthly_queries_rate_limit_limit);
            }

        }

        private static void TestEngagement(EngagementAPI engagement)
        {
            string[] tweetIds = new[] { "741399648594579456" };

            Console.WriteLine("28Hr");
            var response28Hr = engagement.Get28Hr(tweetIds);
            if (response28Hr.ErrorFlag) Console.WriteLine("Error:");
            Console.WriteLine(response28Hr.ResponseString);

            Console.WriteLine("Historical");
            var responseHistorical = engagement.GetHistorical(tweetIds);
            if (responseHistorical.ErrorFlag) Console.WriteLine("Error:");
            Console.WriteLine(responseHistorical.ResponseString);

            Console.WriteLine("Totals");
            var responseTotal = engagement.GetTotals(tweetIds);
            if (responseTotal.ErrorFlag) Console.WriteLine("Error:");
            Console.WriteLine(responseTotal.ResponseString);
        }
    }
}
