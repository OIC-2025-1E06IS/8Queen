# 8 Queens GUI

A Windows Forms desktop interface for the 8 Queens puzzle, written in C#.

## Requirements

- Windows
- Visual Studio 2022 with the **.NET desktop development** workload
- .NET 8 SDK

## Run in Visual Studio

1. Pull the latest changes from the `master` branch.
2. In Solution Explorer, right-click the solution and choose **Add > Existing Project**.
3. Select `GUI/EightQueenGUI.csproj`.
4. Right-click **EightQueenGUI** and choose **Set as Startup Project**.
5. Press **F5** to run.

The original Console App is left unchanged.

## Features

- 8×8 chessboard with alternating square colors
- Click a square to add or remove a queen in that row
- **Solve** displays a valid solution
- **Next Solution** cycles through all 92 valid solutions
- **Reset** clears the board
- Displays the current solution number or number of queens placed
