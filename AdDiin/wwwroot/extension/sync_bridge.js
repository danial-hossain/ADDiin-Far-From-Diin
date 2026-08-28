// Bridge script injected into ADDiin web page: Syncs master switch and toggles with browser extension storage
window.addEventListener("message", (event) => {
  if (event.data && event.data.type === "ADDIIN_SHIELD_UPDATE") {
    chrome.storage.local.set({
      shieldActive: event.data.active,
      platforms: event.data.platforms,
      custom: event.data.custom
    });
  }
});

// Initial read on page load
const currentActive = localStorage.getItem("addiin_shield_active") === "true";
const currentPlatforms = JSON.parse(localStorage.getItem("addiin_blocked_platforms") || "{}");
const currentCustom = JSON.parse(localStorage.getItem("addiin_custom_domains") || "[]");

chrome.storage.local.set({
  shieldActive: currentActive,
  platforms: currentPlatforms,
  custom: currentCustom
});
