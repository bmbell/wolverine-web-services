
# Maze Services

Provides services for generating and solving mazes.

## Building the Services

- Type the command: `dotnet build`

## Running the Services

- Open the solution in Visual Studio
- Click on `Debug -> Start Debugging` (or press the F5 key)

## Testing the Services

- Type the command: `dotnet test`

## Known Issues with Implementation

- The maze endpoints are not truly a REST implementation because true endpoints would require permanant storage, such as a database. Instead, the endpoints should be:
  - **POST** `/mazes` body: { height: number, width: number } - Generates the maze and returns a Maze object with an id
  - **GET** `/mazes/{id}/solution` - Returns the solution to the maze, given the maze's id


