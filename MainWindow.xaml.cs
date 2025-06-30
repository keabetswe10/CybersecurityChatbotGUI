using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CybersecurityChatbotGUI
{
    public partial class MainWindow : Window
    {
        private bool inQuizMode = false;
        private int currentQuestionIndex = 0;
        private int quizScore = 0;

        private List<TaskItem> tasks = new List<TaskItem>();
        private TaskItem currentPendingTask = null;
        private List<string> activityLog = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            PlayWelcomeAudio();

            AddBotMessage("👋 Welcome to the Cybersecurity Awareness Chatbot!\nYou can ask me things like:\n- What is phishing?\n- Add task - Enable 2FA\n- Type 'start quiz' to begin a cybersecurity quiz!");
        }

        private void LogActivity(string action)
        {
            activityLog.Add($"{DateTime.Now:HH:mm} - {action}");
            if (activityLog.Count > 100)
                activityLog.RemoveAt(0);
        }

        private class QuizQuestion
        {
            public string QuestionText { get; set; }
            public string[] Options { get; set; }
            public string Answer { get; set; }
            public string Explanation { get; set; }
        }

        private List<QuizQuestion> quizQuestions = new List<QuizQuestion>
        {
            new QuizQuestion
            {
                QuestionText = "What should you do if you receive an email asking for your password?",
                Options = new[] {"A) Reply with your password", "B) Delete the email", "C) Report the email as phishing", "D) Ignore it"},
                Answer = "C",
                Explanation = "Correct! Reporting phishing emails helps prevent scams."
            },
            new QuizQuestion
            {
                QuestionText = "True or False: You should use the same password for all accounts.",
                Answer = "False",
                Explanation = "False! Reusing passwords puts multiple accounts at risk."
            },
            new QuizQuestion
            {
                QuestionText = "Which is the safest way to create a password?",
                Options = new[] {"A) Your birthdate", "B) Pet’s name", "C) '12345678'", "D) A mix of letters, numbers, and symbols"},
                Answer = "D",
                Explanation = "Correct! Strong passwords use a mix of characters."
            },
            new QuizQuestion
            {
                QuestionText = "True or False: HTTPS is more secure than HTTP.",
                Answer = "True",
                Explanation = "True! HTTPS encrypts data between you and the site."
            },
            new QuizQuestion
            {
                QuestionText = "Which of these is a type of social engineering attack?",
                Options = new[] {"A) Phishing", "B) Malware", "C) Firewall", "D) VPN"},
                Answer = "A",
                Explanation = "Phishing is a form of social engineering."
            },
            new QuizQuestion
            {
                QuestionText = "What should you check before clicking a link in an email?",
                Options = new[] {"A) Nothing", "B) The sender's name", "C) The URL", "D) If it's from a friend"},
                Answer = "C",
                Explanation = "Always hover over links to check the actual URL."
            },
            new QuizQuestion
            {
                QuestionText = "True or False: Public Wi-Fi is always safe to use without a VPN.",
                Answer = "False",
                Explanation = "False! Public Wi-Fi can be dangerous without a VPN."
            },
            new QuizQuestion
            {
                QuestionText = "Which one of these is the most secure login method?",
                Options = new[] {"A) Password only", "B) Face ID", "C) Two-factor authentication", "D) Security questions"},
                Answer = "C",
                Explanation = "2FA adds an extra layer of security."
            },
            new QuizQuestion
            {
                QuestionText = "What is the goal of ransomware?",
                Options = new[] {"A) Make you laugh", "B) Encrypt your files for money", "C) Speed up your PC", "D) Hack your microphone"},
                Answer = "B",
                Explanation = "Ransomware locks files and demands payment."
            },
            new QuizQuestion
            {
                QuestionText = "True or False: You should update your software regularly.",
                Answer = "True",
                Explanation = "True! Updates fix bugs and close security gaps."
            }
        };


        private void Send_Click(object sender, RoutedEventArgs e)
        {
            var input = UserInput.Text.Trim();
            if (!string.IsNullOrEmpty(input))
            {
                AddUserMessage(input);
                var response = GetResponse(input);
                AddBotMessage(response);
                UserInput.Clear();
            }
        }

        private void AddUserMessage(string msg)
        {
            var bubble = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(223, 240, 255)),
                CornerRadius = new CornerRadius(14),
                Padding = new Thickness(10, 6, 10, 6),
                Margin = new Thickness(60, 6, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Right,
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    BlurRadius = 4,
                    ShadowDepth = 1,
                    Color = Colors.LightGray,
                    Opacity = 0.15
                },
                Child = new TextBlock
                {
                    Text = $"🧑‍💻 {msg}",
                    Foreground = Brushes.DarkSlateBlue,
                    FontSize = 15,
                    TextWrapping = TextWrapping.Wrap
                }
            };
            ChatPanel.Children.Add(bubble);
        }

        private void AddBotMessage(string msg)
        {
            var bubble = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(230, 255, 230)),
                CornerRadius = new CornerRadius(14),
                Padding = new Thickness(10, 6, 10, 6),
                Margin = new Thickness(0, 6, 60, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    BlurRadius = 4,
                    ShadowDepth = 1,
                    Color = Colors.LightGray,
                    Opacity = 0.15
                },
                Child = new TextBlock
                {
                    Text = $"🤖 {msg}",
                    Foreground = Brushes.DarkGreen,
                    FontSize = 15,
                    TextWrapping = TextWrapping.Wrap
                }
            };
            ChatPanel.Children.Add(bubble);
        }

        private void PlayWelcomeAudio()
        {
            try
            {
                var player = new SoundPlayer("audio.wav");
                player.Play();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Welcome sound error: " + ex.Message);
            }
        }

        private string AskQuizQuestion()
        {
            var q = quizQuestions[currentQuestionIndex];
            var display = $"Q{currentQuestionIndex + 1}: {q.QuestionText}";
            if (q.Options != null)
            {
                foreach (var opt in q.Options)
                {
                    display += "\n" + opt;
                }
            }
            return display;
        }

        private string RecognizeIntentWithNLP(string input)
        {
            if (Regex.IsMatch(input, @"remind me to (.+) (tomorrow|in \d+ days)"))
            {
                var match = Regex.Match(input, @"remind me to (.+) (tomorrow|in (\d+) days)");
                if (match.Success)
                {
                    var taskTitle = match.Groups[1].Value.Trim();
                    var time = match.Groups[2].Value.ToLower();

                    DateTime reminderDate = DateTime.Now;
                    if (time == "tomorrow")
                        reminderDate = DateTime.Now.AddDays(1);
                    else if (match.Groups.Count > 3 && int.TryParse(match.Groups[3].Value, out int days))
                        reminderDate = DateTime.Now.AddDays(days);

                    var task = new TaskItem
                    {
                        Title = taskTitle,
                        Description = $"Cybersecurity task: {taskTitle}",
                        IsCompleted = false,
                        ReminderDate = reminderDate
                    };

                    tasks.Add(task);
                    LogActivity($"Reminder set: '{taskTitle}' on {reminderDate.ToShortDateString()}");
                    return $"Reminder set for '{taskTitle}' on {reminderDate.ToShortDateString()}.";
                }
            }

            if (Regex.IsMatch(input, @"add (a )?task to (.+)"))
            {
                var match = Regex.Match(input, @"add (a )?task to (.+)");
                if (match.Success)
                {
                    var title = match.Groups[2].Value.Trim();

                    currentPendingTask = new TaskItem
                    {
                        Title = title,
                        Description = $"Cybersecurity task: {title}",
                        IsCompleted = false
                    };
                    LogActivity($"Task added: '{title}'");
                    return $"Task added: '{title}'. Would you like to set a reminder?";
                }
            }
            return null;
        }
        private string GetResponse(string rawInput)
        {
            var input = rawInput.Trim();
            var inputLower = input.ToLower();

            if (inputLower == "start quiz")
            {
                inQuizMode = true;
                currentQuestionIndex = 0;
                quizScore = 0;
                LogActivity("Cybersecurity quiz started.");
                return AskQuizQuestion();
            }
            if (inQuizMode)
            {
                var userAnswer = rawInput.Trim();
                var current = quizQuestions[currentQuestionIndex];
                bool isCorrect = false;

                if (current.Answer.Equals("True", StringComparison.OrdinalIgnoreCase) || current.Answer.Equals("False", StringComparison.OrdinalIgnoreCase))
                {
                    isCorrect = userAnswer.Equals(current.Answer, StringComparison.OrdinalIgnoreCase) || userAnswer.Equals(current.Answer.Substring(0, 1), StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    isCorrect = userAnswer.Equals(current.Answer, StringComparison.OrdinalIgnoreCase);
                }
                var feedback = isCorrect
                    ? $"✅ Correct! {current.Explanation}"
                    : $"❌ Incorrect. {current.Explanation} (Correct answer: {current.Answer})";

                if (isCorrect) quizScore++;
                currentQuestionIndex++;

                if (currentQuestionIndex < quizQuestions.Count)
                    return feedback + "\n\n" + AskQuizQuestion();

                inQuizMode = false;
                LogActivity($"Quiz completed: {quizScore}/{quizQuestions.Count}");
                return feedback + $"\n\n🎉 Quiz Complete! You scored {quizScore} out of {quizQuestions.Count}.\n" +
                       (quizScore >= 7 ? "You're a cybersecurity pro! 🔐" : "Keep learning to stay safe online! 📚") +
                       "\n\nType 'start quiz' to try again.";
            }
            if (currentPendingTask != null)
            {
                if (inputLower.Contains("remind me in"))
                {
                    var match = Regex.Match(input, @"remind me in (\d+) days");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int days))
                    {
                        currentPendingTask.ReminderDate = DateTime.Now.AddDays(days);
                        tasks.Add(currentPendingTask);
                        LogActivity($"Reminder set for task: '{currentPendingTask.Title}' in {days} days");
                        var title = currentPendingTask.Title;
                        currentPendingTask = null;
                        return $"Got it! I'll remind you to '{title}' in {days} days.";
                    }
                }
                tasks.Add(currentPendingTask);
                LogActivity($"Task saved: '{currentPendingTask.Title}' without reminder");
                var savedTitle = currentPendingTask.Title;
                currentPendingTask = null;
                return $"Task '{savedTitle}' saved without a reminder.";
            }
            if (inputLower == "show activity log" || inputLower.Contains("what have you done for me"))
            {
                if (activityLog.Count == 0)
                    return "No activities recorded yet.";
                var recent = activityLog.Skip(Math.Max(0, activityLog.Count - 10)).Reverse();
                string summary = "🗂️ Here's a summary of recent actions:\n\n";
                int count = 1;
                foreach (var entry in recent)
                    summary += $"{count++}. {entry}\n";
                return summary;
            }
            var nlpResult = RecognizeIntentWithNLP(input);
            if (nlpResult != null) return nlpResult;

            if (inputLower.StartsWith("add task -") && input.Length > 10)
            {
                var title = rawInput.Substring("add task -".Length).Trim();
                currentPendingTask = new TaskItem
                {
                    Title = title,
                    Description = $"Cybersecurity task: {title}",
                    IsCompleted = false
                };
                LogActivity($"Task added manually: '{title}'");
                return $"Task added with the description \"{currentPendingTask.Description}\". Would you like a reminder?";
            }
            if (inputLower.Contains("view tasks"))
            {
                if (tasks.Count == 0)
                    return "📝 You have no tasks yet.";

                var list = "📝 Here are your tasks:\n";
                foreach (var task in tasks)
                {
                    list += $"\n- {task.Title} [{(task.IsCompleted ? "✅ Completed" : "❌ Incomplete")}]";
                    if (task.ReminderDate.HasValue)
                        list += $" (Reminder: {task.ReminderDate.Value.ToShortDateString()})";
                }
                return list;
            }
            if (inputLower.StartsWith("complete task -"))
            {
                var title = rawInput.Substring("complete task -".Length).Trim();
                var task = tasks.FirstOrDefault(t => t.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
                if (task != null)
                {
                    task.IsCompleted = true;
                    LogActivity($"Task completed: '{title}'");
                    return $"Marked '{title}' as completed ✅";
                }
                return $"Task '{title}' not found.";
            }
            if (inputLower.StartsWith("delete task -"))
            {
                var title = rawInput.Substring("delete task -".Length).Trim();
                var task = tasks.FirstOrDefault(t => t.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
                if (task != null)
                {
                    tasks.Remove(task);
                    LogActivity($"Task deleted: '{title}'");
                    return $"Deleted task '{title}' 🗑️";
                }
                return $"Task '{title}' not found.";
            }
            if (inputLower.Contains("phishing"))
                return "Phishing is a scam where attackers try to trick you into revealing sensitive information.";
            else if (inputLower.Contains("password"))
                return "A strong password should include uppercase, lowercase, numbers, and symbols.";
            else if (inputLower.Contains("scam"))
                return "Always double-check the URLs of websites you visit.";
            else if (inputLower.Contains("privacy"))
                return "Consider enabling two-factor authentication on all your accounts.";

            return "I'm still learning. You can also manage tasks like:\n- Add task - Enable 2FA\n- View tasks\n- Complete task - Enable 2FA\n- Delete task - Enable 2FA\n- Type 'start quiz' to play the cybersecurity quiz.";
        }
    }
    public class TaskItem
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? ReminderDate { get; set; }
        public bool IsCompleted { get; set; }
    }
}
