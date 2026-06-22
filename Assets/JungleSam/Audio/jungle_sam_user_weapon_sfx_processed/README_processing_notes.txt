Jungle Sam - processed user weapon SFX

{
  "originals": {
    "freesound_community-9mm-pistol-shot-6349.mp3": {
      "duration_ms": 1698,
      "channels": 1,
      "frame_rate": 44100,
      "dBFS": -16.22,
      "max_dBFS": 0.0
    },
    "u_f09vejvoga-gun-shot-350315.mp3": {
      "duration_ms": 8520,
      "channels": 2,
      "frame_rate": 48000,
      "dBFS": -27.28,
      "max_dBFS": -9.22
    }
  },
  "detected": {
    "pistol_nonsilent_ms_threshold_-40": [
      [
        150,
        1065
      ]
    ],
    "gun_nonsilent_ms_threshold_-45": [
      [
        143,
        905
      ],
      [
        1402,
        1898
      ],
      [
        2243,
        3005
      ],
      [
        3502,
        3998
      ],
      [
        4377,
        5139
      ],
      [
        5635,
        6131
      ],
      [
        6477,
        7239
      ],
      [
        7735,
        8231
      ]
    ]
  },
  "processed": {
    "pistol": [
      "pistol_9mm_user_trimmed_game.wav",
      "pistol_9mm_user_trimmed_short.wav"
    ],
    "rifle_full_tail": [
      "rifle_user_single_01_game.wav",
      "rifle_user_single_02_game.wav",
      "rifle_user_single_03_game.wav",
      "rifle_user_single_04_game.wav",
      "rifle_user_single_05_game.wav",
      "rifle_user_single_06_game.wav",
      "rifle_user_single_07_game.wav",
      "rifle_user_single_08_game.wav"
    ],
    "rifle_short": [
      "rifle_user_single_01_short.wav",
      "rifle_user_single_02_short.wav",
      "rifle_user_single_03_short.wav",
      "rifle_user_single_04_short.wav",
      "rifle_user_single_05_short.wav",
      "rifle_user_single_06_short.wav",
      "rifle_user_single_07_short.wav",
      "rifle_user_single_08_short.wav"
    ]
  },
  "unity_recommendations": {
    "Force To Mono": "ON",
    "Load Type": "Decompress On Load",
    "Compression Format": "PCM for test / Vorbis Quality 70-85 for final",
    "Preload Audio Data": "ON",
    "Spatial Blend": "0 for player weapon"
  }
}