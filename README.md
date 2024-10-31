# SweepersHeaven Plugin

**SweepersHeaven** is a quality-of-life plugin for **Supermarket Together** designed to make product collection easier and faster! When thieves leave the store, get beaten and drop a large number of items, chasing after every single one can be exhausting. With **SweepersHeaven**, players can use their broom to collect multiple products at once, saving time and hassle.

## Features

- **Bulk Collection**: Pick up several dropped items with a single sweep.
- **Customizable Collection Radius**: Adjust how wide the sweep is for collected items.
- **Dynamic Visuals**: Collected items are swept aside with added visual impact.
- **Debug Mode**: Special debug options to test product spawning and sweeping functionality.
- **Mini Transporter Pickup**: Collect items using the Mini Transporter.

## Installation

1. Download the latest `.zip` package from [Thunderstore](https://thunderstore.io/c/supermarket-together/p/DooDesch/SweepersHeaven/) or from the releases page.
2. Extract the contents to the `BepInEx/plugins` folder in your **Supermarket Together** installation directory.
3. Launch the game and start sweeping!

## Configuration

After installation, you can configure **SweepersHeaven** using the BepInEx configuration file:

- **Pickup Radius**: Controls how wide an area items are collected from.
- **Max Items per Sweep**: Set a limit on the number of items picked up in one go.
- **Pickup Key**: Assign a custom key to activate the broom for sweeping.
- **Debug Mode**: Toggle debug mode for spawning extra items and testing.
- **Mini Transporter Pickup**: Enable or disable the ability to pick up items with the Mini Transporter.

## Development

To contribute:

1. Clone the repository and build the project with `dotnet build`.
2. Place the compiled `.dll` file from `bin/Debug/netstandard2.1` in your `BepInEx/plugins` folder to test.

## License

Licensed under the MIT License. See the `LICENSE` file for details.

## Questions & Feedback

For questions, suggestions, or feedback, please open an issue on GitHub.
