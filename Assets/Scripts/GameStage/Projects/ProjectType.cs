using System;

//-------------------------------------------------------
// ProjectType
//-------------------------------------------------------
// Purpose:
//   Defines the types of projects (Masharie3) available in Baloot.
//   Each project has different point values and rules.
//-------------------------------------------------------
public enum ProjectType
{
    None = 0,           // No project
    Sara = 1,           // 3 consecutive cards of same suit (20 points)
    Khamsin = 2,        // 4 consecutive cards of same suit (50 points)
    Mia = 3,            // 5 consecutive OR 4 of same rank (10/J/Q/K) OR 4 Aces in Hukm (100 points)
    Arbamiya = 4,       // 4 Aces in Sun (no-trump) (400 points)
    Belote = 5          // K+Q of trump suit (declared during play) (20 points)
}

