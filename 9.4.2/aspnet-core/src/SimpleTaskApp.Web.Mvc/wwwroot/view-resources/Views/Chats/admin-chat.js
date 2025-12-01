document.addEventListener("DOMContentLoaded", () => {
  let selectedUserId = null;
  let selectedUserName = "";
  const usersList = document.getElementById("usersList");
  const chatBody = document.getElementById("chatBody");
  const noChatSelected = document.getElementById("noChatSelected");
  const currentUserName = document.getElementById("currentUserName");
  const currentUserAvatar = document.getElementById("currentUserAvatar");
  const chatInput = document.getElementById("chatInput");
  const sendBtn = document.getElementById("sendBtn");
  const searchInput = document.querySelector('.search-input');
  const deleteHistoryBtn = document.getElementById("deleteHistoryBtn");

  deleteHistoryBtn.style.display = 'none';

  function loadUsers() {
    fetch('/Admin/Chats/GetUsers')
      .then(res => res.json())
      .then(data => {
        const users = data.result || [];
        usersList.innerHTML = '';
        if (!users.length) {
          usersList.innerHTML = `<div class="no-users" style="text-align:center;padding:30px;color:#999;">${L('NoUsers')}</div>`;
          return;
        }
        users.forEach(u => {
          const userItem = document.createElement("div");
          userItem.className = "user-item";
          userItem.dataset.userId = u.id;
          userItem.dataset.userName = u.userName;

          const avatarLetter = u.userName ? u.userName.charAt(0).toUpperCase() : 'U';
          const lastMessage = u.lastMessage || L('NoMessage');
          userItem.innerHTML = `
                        <div class="user-avatar">${avatarLetter}</div>
                        <div class="user-info">
                            <div class="user-name">${u.userName || L('User')}</div>
                            <div class="user-last-message">${lastMessage}</div>
                        </div>
                    `;
          userItem.addEventListener("click", () => selectUser(u.id, u.userName, avatarLetter));
          usersList.appendChild(userItem);
        });
      });
  }

  function selectUser(id, name, avatarLetter = "U") {
    selectedUserId = id;
    selectedUserName = name;

    currentUserName.textContent = name || L('User');
    currentUserAvatar.textContent = avatarLetter;

    noChatSelected.style.display = 'none';
    deleteHistoryBtn.style.display = 'flex';
    chatInput.disabled = false;
    sendBtn.disabled = false;

    document.querySelectorAll('.user-item').forEach(item => item.classList.remove('active'));
    const userEl = document.querySelector(`.user-item[data-user-id="${id}"]`);
    if (userEl) userEl.classList.add('active');

    loadHistory(id);
  }

  const connection = new signalR.HubConnectionBuilder().withUrl("/chatHub?role=admin").build();
  connection.start().then(() => console.log("Admin connected")).catch(err => console.error(err));

  connection.on("ReceiveMessage", (userId, message, type) => {
    if (selectedUserId !== userId) {
      const userItem = document.querySelector(`.user-item[data-user-id="${userId}"]`);
      if (userItem) {
        const lastMsgEl = userItem.querySelector('.user-last-message');
        if (lastMsgEl) lastMsgEl.textContent = message.length > 30 ? message.substring(0, 30) + '...' : message;
      }
      return;
    }
    showMessage(message, type);
    updateUserLastMessage(userId, message);
  });

  function loadHistory(userId) {
    fetch(`/Admin/Chats/GetHistory?userId=${userId}`)
      .then(res => res.json())
      .then(data => {
        const messages = data.result || [];
        chatBody.innerHTML = "";
        if (!messages.length) {
          chatBody.innerHTML = `<div class="no-chat-selected"><div class="no-chat-icon">📨</div><h3>${L('NoMessages')}</h3></div>`;
          return;
        }
        messages.forEach(msg => {
          const messageText = msg.Message || msg.message || L('NoContent');
          const messageType = msg.Sender || msg.sender || "User";
          showMessage(messageText, messageType);
        });
        chatBody.scrollTop = chatBody.scrollHeight;
      });
  }

  function showMessage(message, type) {
    const msgDiv = document.createElement("div");
    msgDiv.className = `message ${type === "User" ? "message-user" : "message-admin"}`;
    msgDiv.textContent = message;
    chatBody.appendChild(msgDiv);
    chatBody.scrollTop = chatBody.scrollHeight;
  }

  function updateUserLastMessage(userId, message) {
    const userItem = document.querySelector(`.user-item[data-user-id="${userId}"]`);
    if (userItem) {
      const lastMsgEl = userItem.querySelector('.user-last-message');
      if (lastMsgEl) lastMsgEl.textContent = message.length > 30 ? message.substring(0, 30) + '...' : message;
    }
  }

  sendBtn.addEventListener("click", () => {
    const message = chatInput.value.trim();
    if (!message || !selectedUserId) return;
    connection.invoke("SendMessageToUser", selectedUserId, message)
      .catch(err => console.error(err));
    showMessage(message, "Admin");
    updateUserLastMessage(selectedUserId, message);
    chatInput.value = "";
    chatInput.style.height = 'auto';
  });

  chatInput.addEventListener("keydown", (e) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      sendBtn.click();
    }
  });

  chatInput.addEventListener("input", function () {
    this.style.height = 'auto';
    this.style.height = Math.min(this.scrollHeight, 120) + 'px';
  });
  deleteHistoryBtn.addEventListener("click", () => {
    if (!selectedUserId) return;

    if (!confirm(`Bạn có chắc muốn xóa lịch sử chat với ${selectedUserName}?`)) return;

    fetch(`/Admin/Chats/DeleteHistory?userId=${selectedUserId}`)
      .then(res => res.json())
      .then(res => {
        if (res.success) {
          chatBody.innerHTML = `
                    <div class="no-chat-selected">
                        <div class="no-chat-icon">✅</div>
                        <h3>Lịch sử đã được xóa</h3>
                    </div>
                `;
          updateUserLastMessage(selectedUserId, "Không có tin nhắn");
        }
      })
      .catch(err => console.error("Xóa lịch sử lỗi:", err));
  });

  searchInput.addEventListener("input", function () {
    const searchTerm = this.value.toLowerCase();
    document.querySelectorAll('.user-item').forEach(item => {
      const name = item.querySelector('.user-name').textContent.toLowerCase();
      const last = item.querySelector('.user-last-message').textContent.toLowerCase();
      item.style.display = name.includes(searchTerm) || last.includes(searchTerm) ? 'flex' : 'none';
    });
  });

  loadUsers();
});
