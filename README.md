# AutoTracker
Extensible automated tracker to display current equipment or progress, ideal for streaming randomized games. Custom tracker layouts can be created by editing images and JSON files. Custom layouts can be used for manual tracking or can even be automated.

Written primarily as an automatic tracker for [Z1M1](https://z1m1.info/).

## Planned features

JSON-Based Extensibility:
  - Easily editable JSON-based tracker layouts for custom trackers
  - Easily editable JSON-based "association" files that automate above trackers by
    associating monitored game data with tracker layout elements
  - Leverage extensions provided by .NET components
    
.NET-Based Extensibility:
  - Implement complex logic to update tracker layout
  - Provide additional "hooking" mechanisms to monitor progress of different games
  - Provide functions that can be utilized by JSON files
      - e.g. `{ "totalLives" : { "@sum": [ "player1.lives", "player2.lives"] } }`
