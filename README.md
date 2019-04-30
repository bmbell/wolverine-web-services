
# Maze Game



## Getting Started



## Known Issues with Implementation

- The maze endpoints are not truly a REST implementation because true endpoints would require permanant storage, such as a database. Instead, the endpoints should be:
  - **POST** `/mazes` body: { height: number, width: number } - Generates the maze and returns a Maze object with an id
  - **GET** `/mazes/{id}/solution` - Returns the solution to the maze, given the maze's id

## Enhancements

