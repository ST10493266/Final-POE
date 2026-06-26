using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace CybersecurityAwarenessBotGUI;

public sealed class MainForm : Form
{
    private readonly TaskRepository _repository = new();
    private readonly ActivityLog _activityLog = new();
    private readonly QuizService _quizService = new();
    private readonly ChatBotService _chatBot;

    private readonly RichTextBox _chatHistory = new();
    private readonly TextBox _chatInput = new();
    private readonly DataGridView _taskGrid = new();
    private readonly TextBox _taskTitle = new();
    private readonly TextBox _taskDescription = new();
    private readonly DateTimePicker _reminderDate = new();
    private readonly CheckBox _useReminder = new();
    private readonly ListBox _activityList = new();
    private readonly Label _quizQuestion = new();
    private readonly FlowLayoutPanel _quizOptions = new();
    private readonly Label _quizFeedback = new();
    private readonly Label _quizScore = new();
    private readonly TabControl _tabs = new();

    private int _quizIndex;
    private int _quizCorrect;

    public MainForm()
    {
        _chatBot = new ChatBotService(_repository, _activityLog);
        _chatBot.TasksChanged += RefreshTasks;
        _chatBot.LogRequested += () => _tabs.SelectedTab = _tabs.TabPages["Activity"];
        _chatBot.QuizRequested += () => { _tabs.SelectedTab = _tabs.TabPages["Quiz"]; StartQuiz(); };

        Text = "Cybersecurity Awareness Chatbot";
        MinimumSize = new Size(1050, 720);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(244, 247, 250);

        BuildInterface();
        InitializeDatabase();
        AddBotMessage("Welcome. I can help you manage cybersecurity tasks, reminders, quiz practice, and safety questions.");
    }

    private void BuildInterface()
    {
        var header = new Panel { Dock = DockStyle.Top, Height = 76, BackColor = Color.FromArgb(18, 37, 63) };
        header.Controls.Add(new Label { Text = "Cybersecurity Awareness Chatbot", ForeColor = Color.White, Font = new Font("Segoe UI", 20, FontStyle.Bold), AutoSize = true, Location = new Point(24, 20) });
        Controls.Add(header);

        _tabs.Dock = DockStyle.Fill;
        _tabs.Font = new Font("Segoe UI", 10);
        Controls.Add(_tabs);
        _tabs.TabPages.Add(BuildChatTab());
        _tabs.TabPages.Add(BuildTasksTab());
        _tabs.TabPages.Add(BuildQuizTab());
        _tabs.TabPages.Add(BuildActivityTab());
    }

    private TabPage BuildChatTab()
    {
        var page = new TabPage("Chat") { Name = "Chat", BackColor = Color.White };
        _chatHistory.Dock = DockStyle.Fill;
        _chatHistory.ReadOnly = true;
        _chatHistory.BorderStyle = BorderStyle.None;
        _chatHistory.Font = new Font("Segoe UI", 11);
        _chatHistory.BackColor = Color.White;

        var bottom = new Panel { Dock = DockStyle.Bottom, Height = 64, Padding = new Padding(16, 10, 16, 10), BackColor = Color.FromArgb(236, 241, 246) };
        _chatInput.Dock = DockStyle.Fill;
        _chatInput.Font = new Font("Segoe UI", 11);
        _chatInput.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; SendChat(); } };

        var send = PrimaryButton("Send", 110);
        send.Dock = DockStyle.Right;
        send.Click += (_, _) => SendChat();
        bottom.Controls.Add(_chatInput);
        bottom.Controls.Add(send);
        page.Controls.Add(_chatHistory);
        page.Controls.Add(bottom);
        return page;
    }

    private TabPage BuildTasksTab()
    {
        var page = new TabPage("Tasks") { Name = "Tasks", BackColor = Color.White };
        var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 350, BackColor = Color.White };
        var form = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(18), RowCount = 8, ColumnCount = 1 };
        form.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        form.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        form.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        form.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        form.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        form.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        form.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        form.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        form.Controls.Add(new Label { Text = "Add Cybersecurity Task", Font = new Font("Segoe UI", 15, FontStyle.Bold), AutoSize = true });
        form.Controls.Add(new Label { Text = "Title", AutoSize = true, Margin = new Padding(0, 16, 0, 4) });
        _taskTitle.Dock = DockStyle.Top;
        form.Controls.Add(_taskTitle);
        form.Controls.Add(new Label { Text = "Description", AutoSize = true, Margin = new Padding(0, 12, 0, 4) });
        _taskDescription.Multiline = true;
        _taskDescription.Height = 120;
        _taskDescription.Dock = DockStyle.Fill;
        form.Controls.Add(_taskDescription);
        _useReminder.Text = "Set reminder";
        _useReminder.AutoSize = true;
        _useReminder.Margin = new Padding(0, 12, 0, 4);
        form.Controls.Add(_useReminder);
        _reminderDate.Format = DateTimePickerFormat.Short;
        _reminderDate.MinDate = DateTime.Today;
        form.Controls.Add(_reminderDate);
        var add = PrimaryButton("Add Task", 140);
        add.Dock = DockStyle.Top;
        add.Margin = new Padding(0, 16, 0, 0);
        add.Click += (_, _) => AddTaskFromForm();
        form.Controls.Add(add);

        _taskGrid.Dock = DockStyle.Fill;
        _taskGrid.ReadOnly = true;
        _taskGrid.AllowUserToAddRows = false;
        _taskGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _taskGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _taskGrid.BackgroundColor = Color.White;

        var actions = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 56, Padding = new Padding(12), FlowDirection = FlowDirection.LeftToRight };
        var complete = new Button { Text = "Mark Completed", Width = 150, Height = 32 };
        var delete = new Button { Text = "Delete", Width = 100, Height = 32 };
        complete.Click += (_, _) => MarkSelectedTaskCompleted();
        delete.Click += (_, _) => DeleteSelectedTask();
        actions.Controls.Add(complete);
        actions.Controls.Add(delete);

        split.Panel1.Controls.Add(form);
        split.Panel2.Controls.Add(_taskGrid);
        split.Panel2.Controls.Add(actions);
        page.Controls.Add(split);
        return page;
    }

    private TabPage BuildQuizTab()
    {
        var page = new TabPage("Quiz") { Name = "Quiz", BackColor = Color.White };
        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(24), RowCount = 5, ColumnCount = 1 };
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var start = PrimaryButton("Start Quiz", 130);
        start.Click += (_, _) => StartQuiz();
        panel.Controls.Add(start);
        _quizScore.Font = new Font("Segoe UI", 11, FontStyle.Bold);
        _quizScore.AutoSize = true;
        _quizScore.Margin = new Padding(0, 14, 0, 8);
        panel.Controls.Add(_quizScore);
        _quizQuestion.Font = new Font("Segoe UI", 15, FontStyle.Bold);
        _quizQuestion.AutoSize = true;
        _quizQuestion.MaximumSize = new Size(900, 0);
        panel.Controls.Add(_quizQuestion);
        _quizOptions.Dock = DockStyle.Fill;
        _quizOptions.FlowDirection = FlowDirection.TopDown;
        _quizOptions.WrapContents = false;
        panel.Controls.Add(_quizOptions);
        _quizFeedback.Font = new Font("Segoe UI", 11);
        _quizFeedback.AutoSize = true;
        _quizFeedback.Margin = new Padding(0, 12, 0, 0);
        panel.Controls.Add(_quizFeedback);
        page.Controls.Add(panel);
        return page;
    }

    private TabPage BuildActivityTab()
    {
        var page = new TabPage("Activity") { Name = "Activity", BackColor = Color.White };
        _activityList.Dock = DockStyle.Fill;
        _activityList.Font = new Font("Segoe UI", 11);
        _activityList.BorderStyle = BorderStyle.None;
        page.Controls.Add(_activityList);
        return page;
    }

    private static Button PrimaryButton(string text, int width)
    {
        var button = new Button { Text = text, Width = width, Height = 38, BackColor = Color.FromArgb(26, 115, 232), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        button.FlatAppearance.BorderSize = 0;
        return button;
    }

    private void InitializeDatabase()
    {
        try { _repository.Initialize(); _activityLog.Add("Connected to MySQL task database."); RefreshTasks(); }
        catch (Exception ex) { AddBotMessage("MySQL is not connected yet. Please start MySQL and check the connection string in TaskRepository.cs. " + ex.Message); _activityLog.Add("MySQL connection failed: " + ex.Message); }
        RefreshActivity();
    }

    private void SendChat()
    {
        var text = _chatInput.Text.Trim();
        if (string.IsNullOrWhiteSpace(text)) return;
        AddUserMessage(text);
        _chatInput.Clear();
        try { AddBotMessage(_chatBot.Respond(text)); }
        catch (Exception ex) { AddBotMessage("I could not complete that action. Check that MySQL is running, then try again. " + ex.Message); }
        RefreshActivity();
    }

    private void AddTaskFromForm()
    {
        if (string.IsNullOrWhiteSpace(_taskTitle.Text)) { MessageBox.Show("Please enter a task title.", "Missing title", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
        try
        {
            var task = _repository.Add(new CyberTask { Title = _taskTitle.Text.Trim(), Description = string.IsNullOrWhiteSpace(_taskDescription.Text) ? "Cybersecurity task added from the GUI." : _taskDescription.Text.Trim(), ReminderDate = _useReminder.Checked ? _reminderDate.Value.Date : null });
            _activityLog.Add($"Task added from GUI: '{task.Title}'.");
            _taskTitle.Clear(); _taskDescription.Clear(); _useReminder.Checked = false;
            RefreshTasks(); RefreshActivity();
        }
        catch (Exception ex) { MessageBox.Show("Task could not be saved. Check MySQL and try again.\n\n" + ex.Message, "Database error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void MarkSelectedTaskCompleted()
    {
        var id = SelectedTaskId();
        if (id is null) return;
        _repository.MarkCompleted(id.Value);
        _activityLog.Add($"Task marked as completed: ID {id.Value}.");
        RefreshTasks(); RefreshActivity();
    }

    private void DeleteSelectedTask()
    {
        var id = SelectedTaskId();
        if (id is null) return;
        _repository.Delete(id.Value);
        _activityLog.Add($"Task deleted: ID {id.Value}.");
        RefreshTasks(); RefreshActivity();
    }

    private int? SelectedTaskId()
    {
        if (_taskGrid.SelectedRows.Count == 0) { MessageBox.Show("Please select a task first.", "No task selected", MessageBoxButtons.OK, MessageBoxIcon.Information); return null; }
        return Convert.ToInt32(_taskGrid.SelectedRows[0].Cells["Id"].Value);
    }

    private void RefreshTasks()
    {
        try
        {
            _taskGrid.DataSource = _repository.GetAll().Select(task => new { task.Id, task.Title, task.Description, Reminder = task.ReminderDate?.ToString("yyyy-MM-dd") ?? "", Status = task.IsCompleted ? "Completed" : "Pending", Created = task.CreatedAt.ToString("yyyy-MM-dd") }).ToList();
        }
        catch { _taskGrid.DataSource = null; }
    }

    private void RefreshActivity()
    {
        _activityList.Items.Clear();
        foreach (var entry in _activityLog.Recent(10)) _activityList.Items.Add(entry);
    }

    private void StartQuiz()
    {
        _quizIndex = 0; _quizCorrect = 0;
        _activityLog.Add("Quiz started.");
        ShowQuestion(); RefreshActivity();
    }

    private void ShowQuestion()
    {
        _quizOptions.Controls.Clear();
        _quizFeedback.Text = "";
        if (_quizIndex >= _quizService.Questions.Count)
        {
            var total = _quizService.Questions.Count;
            var message = _quizCorrect >= total * 0.75 ? "Great job! You're a cybersecurity pro." : "Keep learning to stay safe online.";
            _quizQuestion.Text = $"Quiz complete. Final score: {_quizCorrect}/{total}. {message}";
            _quizScore.Text = "";
            _activityLog.Add($"Quiz completed with score {_quizCorrect}/{total}.");
            RefreshActivity();
            return;
        }

        var question = _quizService.Questions[_quizIndex];
        _quizScore.Text = $"Question {_quizIndex + 1} of {_quizService.Questions.Count} | Score: {_quizCorrect}";
        _quizQuestion.Text = question.Question;
        for (var i = 0; i < question.Options.Count; i++)
        {
            var index = i;
            var option = new Button { Text = $"{(char)('A' + i)}) {question.Options[i]}", Width = 680, Height = 42, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 11), Margin = new Padding(0, 5, 0, 5) };
            option.Click += (_, _) => AnswerQuestion(index);
            _quizOptions.Controls.Add(option);
        }
    }

    private void AnswerQuestion(int selectedIndex)
    {
        var question = _quizService.Questions[_quizIndex];
        var correct = selectedIndex == question.CorrectIndex;
        if (correct) _quizCorrect++;
        _quizFeedback.Text = (correct ? "Correct! " : "Incorrect. ") + question.Explanation;
        _activityLog.Add($"Quiz question {_quizIndex + 1} answered {(correct ? "correctly" : "incorrectly")}.");
        var timer = new Timer { Interval = 1300 };
        timer.Tick += (_, _) => { timer.Stop(); timer.Dispose(); _quizIndex++; ShowQuestion(); };
        timer.Start();
        RefreshActivity();
    }

    private void AddUserMessage(string message)
    {
        _chatHistory.SelectionColor = Color.FromArgb(180, 24, 24); _chatHistory.AppendText("User: ");
        _chatHistory.SelectionColor = Color.Black; _chatHistory.AppendText(message + Environment.NewLine + Environment.NewLine);
    }

    private void AddBotMessage(string message)
    {
        _chatHistory.SelectionColor = Color.FromArgb(0, 120, 55); _chatHistory.AppendText("Chatbot: ");
        _chatHistory.SelectionColor = Color.Black; _chatHistory.AppendText(message + Environment.NewLine + Environment.NewLine);
    }
}
