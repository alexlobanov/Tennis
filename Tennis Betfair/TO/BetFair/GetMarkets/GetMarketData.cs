using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Tennis_Betfair.TO.BetFair.GetMarkets
{
    public class Runners
    {
        [JsonProperty("runner1Name")]
        public string runner1Name { get; set; }

        [JsonProperty("runner2Name")]
        public string runner2Name { get; set; }

        [JsonProperty("runner1SelectionId")]
        public int runner1SelectionId { get; set; }

        [JsonProperty("runner2SelectionId")]
        public int runner2SelectionId { get; set; }

        [JsonProperty("runner1ShortName")]
        public string runner1ShortName { get; set; }

        [JsonProperty("runner2ShortName")]
        public string runner2ShortName { get; set; }
    }

    public class Radio
    {
        [JsonProperty("url")]
        public string url { get; set; }
    }

    public class BfLiveVideo
    {
    }

    public class Broadcasts
    {
        [JsonProperty("tv")]
        public IList<object> tv { get; set; }

        [JsonProperty("radio")]
        public Radio radio { get; set; }

        [JsonProperty("bfLiveVideo")]
        public BfLiveVideo bfLiveVideo { get; set; }

        [JsonProperty("isLiveVideoAvailable")]
        public bool isLiveVideoAvailable { get; set; }

        [JsonProperty("isDataVisualizationAvailable")]
        public bool isDataVisualizationAvailable { get; set; }
    }

    public class MatchInfo
    {
        [JsonProperty("surface")]
        public string surface { get; set; }

        [JsonProperty("numberOfSets")]
        public string numberOfSets { get; set; }
    }

    public class Home
    {
        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("score")]
        public string score { get; set; }

        [JsonProperty("halfTimeScore")]
        public string halfTimeScore { get; set; }

        [JsonProperty("fullTimeScore")]
        public string fullTimeScore { get; set; }

        [JsonProperty("penaltiesScore")]
        public string penaltiesScore { get; set; }

        [JsonProperty("penaltiesSequence")]
        public IList<object> penaltiesSequence { get; set; }

        [JsonProperty("games")]
        public string games { get; set; }

        [JsonProperty("sets")]
        public string sets { get; set; }

        [JsonProperty("aces")]
        public string aces { get; set; }

        [JsonProperty("doubleFaults")]
        public string doubleFaults { get; set; }

        [JsonProperty("gameSequence")]
        public IList<string> gameSequence { get; set; }

        [JsonProperty("isServing")]
        public bool isServing { get; set; }

        [JsonProperty("highlight")]
        public bool highlight { get; set; }

        [JsonProperty("serviceBreaks")]
        public int serviceBreaks { get; set; }

        [JsonProperty("playerSeed")]
        public int? playerSeed { get; set; }
    }

    public class Away
    {
        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("score")]
        public string score { get; set; }

        [JsonProperty("halfTimeScore")]
        public string halfTimeScore { get; set; }

        [JsonProperty("fullTimeScore")]
        public string fullTimeScore { get; set; }

        [JsonProperty("penaltiesScore")]
        public string penaltiesScore { get; set; }

        [JsonProperty("penaltiesSequence")]
        public IList<object> penaltiesSequence { get; set; }

        [JsonProperty("games")]
        public string games { get; set; }

        [JsonProperty("sets")]
        public string sets { get; set; }

        [JsonProperty("aces")]
        public string aces { get; set; }

        [JsonProperty("doubleFaults")]
        public string doubleFaults { get; set; }

        [JsonProperty("gameSequence")]
        public IList<string> gameSequence { get; set; }

        [JsonProperty("isServing")]
        public bool isServing { get; set; }

        [JsonProperty("highlight")]
        public bool highlight { get; set; }

        [JsonProperty("serviceBreaks")]
        public int serviceBreaks { get; set; }

        [JsonProperty("playerSeed")]
        public int? playerSeed { get; set; }
    }

    public class Score
    {
        [JsonProperty("home")]
        public Home home { get; set; }

        [JsonProperty("away")]
        public Away away { get; set; }
    }

    public class FullTimeElapsed
    {
        [JsonProperty("hour")]
        public int hour { get; set; }

        [JsonProperty("min")]
        public int min { get; set; }

        [JsonProperty("sec")]
        public int sec { get; set; }
    }

    public class State
    {
        [JsonProperty("eventTypeId")]
        public int eventTypeId { get; set; }

        [JsonProperty("eventId")]
        public int eventId { get; set; }

        [JsonProperty("score")]
        public Score score { get; set; }

        [JsonProperty("currentSet")]
        public int currentSet { get; set; }

        [JsonProperty("currentGame")]
        public int currentGame { get; set; }

        [JsonProperty("hasSets")]
        public bool hasSets { get; set; }

        [JsonProperty("fullTimeElapsed")]
        public FullTimeElapsed fullTimeElapsed { get; set; }

        [JsonProperty("matchStatus")]
        public string matchStatus { get; set; }

        [JsonProperty("currentPoint")]
        public int? currentPoint { get; set; }
    }

    public class GetMarketData
    {
        [JsonProperty("eventId")]
        public int eventId { get; set; }

        [JsonProperty("eventTypeId")]
        public int eventTypeId { get; set; }

        [JsonProperty("marketId")]
        public string marketId { get; set; }

        [JsonProperty("marketName")]
        public string marketName { get; set; }

        [JsonProperty("relevance")]
        public int relevance { get; set; }

        [JsonProperty("eventName")]
        public string eventName { get; set; }

        [JsonProperty("competitionName")]
        public string competitionName { get; set; }

        [JsonProperty("competitionId")]
        public string competitionId { get; set; }

        [JsonProperty("numberOfRunners")]
        public int numberOfRunners { get; set; }

        [JsonProperty("startTime")]
        public DateTime startTime { get; set; }

        [JsonProperty("runners")]
        public Runners runners { get; set; }

        [JsonProperty("homeName")]
        public string homeName { get; set; }

        [JsonProperty("awayName")]
        public string awayName { get; set; }

        [JsonProperty("broadcasts")]
        public Broadcasts broadcasts { get; set; }

        [JsonProperty("matchInfo")]
        public MatchInfo matchInfo { get; set; }

        [JsonProperty("inPlayBettingStatus")]
        public string inPlayBettingStatus { get; set; }

        [JsonProperty("state")]
        public State state { get; set; }

        [JsonProperty("countryCode")]
        public string countryCode { get; set; }

        [JsonProperty("shortHomeName")]
        public string shortHomeName { get; set; }

        [JsonProperty("shortAwayName")]
        public string shortAwayName { get; set; }
    }
}