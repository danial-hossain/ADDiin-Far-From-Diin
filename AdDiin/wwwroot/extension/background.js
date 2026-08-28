// ADDiin Far Extension Background Worker (Manifest V3)
const DEFAULT_BLOCKED = {
  facebook: ["facebook.com", "messenger.com"],
  instagram: ["instagram.com"],
  x: ["x.com", "twitter.com"],
  tiktok: ["tiktok.com"],
  youtube: ["youtube.com"]
};

let temporaryPass = {};

async function isBlockedUrl(urlString) {
  if (!urlString) return false;
  try {
    const data = await chrome.storage.local.get(["shieldActive", "platforms", "custom"]);
    
    // If shield is explicitly turned OFF, allow everything!
    if (data.shieldActive === false) {
      return false;
    }

    const urlObj = new URL(urlString);
    const hostname = urlObj.hostname.toLowerCase();

    // Check if domain has active temporary unblock pass
    if (temporaryPass[hostname] && temporaryPass[hostname] > Date.now()) {
      return false;
    }

    // Build active blocked domains list
    let activeDomains = [];
    const platforms = data.platforms || {};

    for (const [key, domains] of Object.entries(DEFAULT_BLOCKED)) {
      if (platforms[key] !== false) {
        activeDomains.push(...domains);
      }
    }

    if (Array.isArray(data.custom)) {
      activeDomains.push(...data.custom);
    }

    return activeDomains.some(d => hostname === d || hostname.endsWith("." + d));
  } catch (e) {
    return false;
  }
}

async function handleBlock(tabId, urlString) {
  const shouldBlock = await isBlockedUrl(urlString);
  if (shouldBlock) {
    const urlObj = new URL(urlString);
    const blockedPageUrl = chrome.runtime.getURL(`blocked.html?domain=${encodeURIComponent(urlObj.hostname)}`);
    chrome.tabs.update(tabId, { url: blockedPageUrl });
  }
}

// 1. Intercept tab updates
chrome.tabs.onUpdated.addListener((tabId, changeInfo, tab) => {
  const url = changeInfo.url || tab.url;
  if (url) {
    handleBlock(tabId, url);
  }
});

// 2. Intercept on navigation
if (chrome.webNavigation) {
  chrome.webNavigation.onBeforeNavigate.addListener(async (details) => {
    if (details.frameId === 0) {
      const shouldBlock = await isBlockedUrl(details.url);
      if (shouldBlock) {
        handleBlock(details.tabId, details.url);
      }
    }
  });
}

// 3. Listen for temporary pass requests
chrome.runtime.onMessage.addListener((request, sender, sendResponse) => {
  if (request.type === "TEMPORARY_PASS" && request.domain) {
    temporaryPass[request.domain] = Date.now() + (5 * 60 * 1000);
    sendResponse({ success: true });
  }
});
