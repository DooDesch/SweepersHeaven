# SweepersHeaven Plugin

**SweepersHeaven** is a plugin for **Supermarket Together** that allows players to collect items in the supermarket more efficiently when using a broom. Perfect for players who want a tidier gameplay experience!

## Features

- **Mass Collection**: Collect multiple items at once with a single click.
- **Adjustable Collection Radius**: Define the range for collected items.
- **Physical Effects**: Items fly aside when collected.
- **Debug Mode**: Includes special debugging options for testing.

## Installation

1. Download the `.zip` package from [Thunderstore](https://thunderstore.io/) or from the releases page.
2. Extract the files into the `BepInEx/plugins` folder of your **Supermarket Together** installation.
3. Start the game!

## Configuration

Options can be adjusted in the BepInEx configuration folder:

- **Pickup Radius**: Set the collection radius.
- **Max Items to Pick**: Set the max items per action, that will be picked up.
- **Pickup Key**: Configure the key for broom activation.
- **Debug Mode**: Enable debug mode to spawn 100 products from your shelfs on the ground.

## Development

To contribute to the plugin:

1. Clone the repository.
2. Use `dotnet build` to compile the project.
3. Copy the compiled `.dll` file in `bin/Debug/netstandard2.1` to the `BepInEx/plugins` folder for testing.

## License

This project is licensed under the MIT License. See `LICENSE` for more details.

## Contact

For questions or suggestions, reach out via GitHub Issues.
