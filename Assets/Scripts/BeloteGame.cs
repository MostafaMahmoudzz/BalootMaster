using Pebble; 

//-------------------------------------------------------
// BeloteGame
//-------------------------------------------------------
// Purpose:
//   Entry-point StageComponent that wires a `GameStage` (runtime logic)
//   with a `GameStageDefinition` (data/config). This component is
//   typically used by the Pebble framework to bootstrap and run the
 //
// How it connects to other scripts:
//   - `Gamew (players, dealing, turns,
//     folds, scoring, UI renderer). This component hosts/owns it.
//   - `GameStageDefinition` provides rules and static data (dealing
//     rules, scoring data) used by `GameStage`.
//   - Player-related scripts (`Player`, `AIPlayer`, `HumanPlayer`) are
//     constructed and driven by `GameStage` during runtime.
//   - Card-related scripts (`BeloteCard`, `BeloteDeck`, `Fold`, etc.)
//     are used by `GameStage` to execute a Belote round.
//
// Notes:
//   The class is intentionally minimal because `StageComponent<TStage,
//   TDefinition>` handles the lifecycle integration with Pebble. The
//   heavy lifting resides in `GameStage`.
//-------------------------------------------------------
public class BeloteGame : StageComponent<GameStage, GameStageDefinition>
{
    // Class body intentionally empty:
    // - The base `StageComponent` implements the lifecycle glue.
    // - `GameStage` contains the actual runtime logic.
}