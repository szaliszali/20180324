﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Trivia
{
    public class Game
    {
        private readonly IUi mUi;
        private readonly ITimer mTimer;

        private readonly List<Player> mPlayers = new List<Player>();

        private readonly LinkedList<string> mPopQuestions = new LinkedList<string>();
        private readonly LinkedList<string> mRockQuestions = new LinkedList<string>();
        private readonly LinkedList<string> mScienceQuestions = new LinkedList<string>();
        private readonly LinkedList<string> mSportsQuestions = new LinkedList<string>();
        private int mCurrentPlayerIndex;
        private Player mCurrentPlayer;
        private bool mIsGettingOutOfPenaltyBox;

        public Game(IUi ui, ITimer timer)
        {
            mUi = ui;
            mTimer = timer;

            for (var i = 0; i < 50; i++)
            {
                mPopQuestions.AddLast("Pop Question " + i);
                mScienceQuestions.AddLast("Science Question " + i);
                mSportsQuestions.AddLast("Sports Question " + i);
                mRockQuestions.AddLast("Rock Question " + i);
            }
        }

        public bool Add(Player player)
        {
            mPlayers.Add(player);

            mUi.PlayerAdded(player.Name, mPlayers.Count);
            return true;
        }

        public void Roll(int roll)
        {
            mCurrentPlayer = mPlayers[mCurrentPlayerIndex];

            var playerName = mCurrentPlayer.Name;
            mUi.PlayerRolls(playerName, roll);

            if (mCurrentPlayer.IsInPenalty)
            {
                if (roll % 2 != 0)
                {
                    mIsGettingOutOfPenaltyBox = true;

                    mUi.PlayerLeavesPenalty(playerName);
                    Advance(roll);
                }
                else
                {
                    mUi.PlayerStaysInPenalty(playerName);
                    mIsGettingOutOfPenaltyBox = false;
                }
            }
            else
            {
                Advance(roll);
            }
        }

        private void Advance(int roll)
        {
            mCurrentPlayer.Advance(roll);
            AskQuestion();
        }

        private void AskQuestion()
        {
            var currentCategory = mCurrentPlayer.CurrentCategory();
            if (currentCategory == "Pop") Question(mPopQuestions);
            if (currentCategory == "Science") Question(mScienceQuestions);
            if (currentCategory == "Sports") Question(mSportsQuestions);
            if (currentCategory == "Rock") Question(mRockQuestions);
        }

        private static void Question(LinkedList<string> questions)
        {
            Console.WriteLine(questions.First());
            questions.RemoveFirst();
        }

        public bool WasCorrectlyAnswered()
        {
            if (mCurrentPlayer.IsInPenalty && !mIsGettingOutOfPenaltyBox)
            {
                NextPlayer();
                return true;
            }

            mCurrentPlayer.CorrectAnswer();

            var winner = !mCurrentPlayer.Won;
            if (!winner) mUi.PlayerWon(mCurrentPlayer.Name);
            NextPlayer();

            return winner;
        }

        public bool WrongAnswer()
        {
            mCurrentPlayer.WrongAnswer();

            NextPlayer();
            return true;
        }

        private void NextPlayer()
        {
            mCurrentPlayerIndex++;
            if (mCurrentPlayerIndex == mPlayers.Count) mCurrentPlayerIndex = 0;
        }
    }

    public interface ITimer
    {
        bool Timeout { get; }
    }
}