(function () {
    'use strict';

    // لما الإضافة تتحمل
    Office.onReady(function (info) {
        if (info.host === Office.HostType.Outlook) {
            document.getElementById("sendBtn").onclick = sendToApi;
        }
    });

    // الدالة الأساسية: تقرأ البريد وتبعته للـ API
    function sendToApi() {
        const item = Office.context.mailbox.item;

        if (!item) {
            showStatus("No email open", "error");
            return;
        }

        showStatus("Reading email...", "");

        // قراءة النص الكامل للبريد أولاً
        item.body.getAsync(Office.CoercionType.Text, function (result) {
            let fullBody = result.status === Office.AsyncResultStatus.Succeeded
                ? result.value
                : item.bodyPreview || "";

            // تجميع بيانات البريد
            let emailData = {
                OutlookItemId: item.itemId,
                Subject: item.subject,
                Sender: {
                    Email: item.from ? item.from.emailAddress : null,
                    Name: item.from ? item.from.displayName : null
                },
                ToRecipients: item.to ? item.to.map(r => r.emailAddress) : [],
                CcRecipients: item.cc ? item.cc.map(r => r.emailAddress) : [],
                BodyPreview: item.bodyPreview || "",
                FullBody: fullBody,
                ConversationId: item.conversationId,
                DateTimeCreated: item.dateTimeCreated,
                Attachments: item.attachments ? item.attachments.map(att => ({
                    Name: att.name,
                    Id: att.id,
                    Size: att.size,
                    AttachmentType: att.attachmentType
                })) : []
            };

            // إرسال للـ API
            callApi(emailData);
        });
    }

    // الاتصال بالـ API
    function callApi(emailData) {
        const apiUrl = "https://10.0.10.159:2713/api/Emails/CreateTicketFromEmail";

        fetch(apiUrl, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(emailData)
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }
                return response.json();
            })
            .then(data => {
                showStatus("✅ Sent successfully!", "success");

                // فتح صفحة التفاصيل لو فيه ID
                if (data.tempEmailId) {
                    setTimeout(() => {
                        window.open(`https://10.0.10.159:2713/Ticket/CreateFromEmail/${data.tempEmailId}`, '_blank');
                    }, 1500);
                }
            })
            .catch(error => {
                console.error('Error:', error);
                showStatus("❌ Failed: " + error.message, "error");
            });
    }

    // عرض الحالة للمستخدم
    function showStatus(message, type) {
        const statusDiv = document.getElementById("status");
        statusDiv.innerHTML = message;
        statusDiv.className = "status " + type;
    }
})();
