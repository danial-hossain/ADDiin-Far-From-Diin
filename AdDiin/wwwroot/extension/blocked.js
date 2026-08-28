// ADDiin Blocked Screen Interaction Script (Chrome CSP compliant)
document.addEventListener("DOMContentLoaded", () => {
  const params = new URLSearchParams(window.location.search);
  const domain = params.get('domain') || 'Social Media';
  
  const domainTitle = document.getElementById('domainTitle');
  if (domainTitle) {
    domainTitle.textContent = domain + ' is Blocked';
  }

  const linkApp = document.getElementById('linkApp');
  if (linkApp) {
    linkApp.href = "http://localhost:5091/focus";
  }

  const unblockBtn = document.getElementById('btnUnblock');
  if (unblockBtn) {
    unblockBtn.addEventListener('click', () => {
      if (domain && domain !== 'Social Media') {
        chrome.runtime.sendMessage({ type: "TEMPORARY_PASS", domain: domain }, (res) => {
          window.location.href = 'https://' + domain;
        });
      } else {
        window.history.back();
      }
    });
  }
});
