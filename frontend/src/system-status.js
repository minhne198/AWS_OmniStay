(function () {
  const api = window.OmniStayApi;
  const ui = window.OmniStaySession;
  const session = ui.requireAuth({ role: 'Admin' });
  if (!session) {
    return;
  }

  ui.hydrateNav('status');

  const target = document.getElementById('statusPanel');
  const refreshButton = document.getElementById('refreshStatus');

  refreshButton.addEventListener('click', loadStatus);

  async function loadStatus() {
    ui.renderMessage(target, 'Đang kiểm tra runtime...', 'muted');

    try {
      const status = await api.getAwsStatus();
      target.className = 'grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4';
      target.hidden = false;
      target.innerHTML = [
        ['Environment', status.environment],
        ['Region', status.region],
        ['Database', status.databaseProvider],
        ['Redis configured', status.redisConfigured ? 'Yes' : 'No'],
        ['Redis connected', status.redisConnected ? 'Yes' : 'No'],
        ['Cache TTL', `${status.searchCacheTtlSeconds}s`],
        ['S3 bucket', status.s3FrontendBucket],
        ['CloudFront', status.cloudFrontDomain],
        ['API base path', status.apiBasePath]
      ].map(([label, value]) => `
        <article class="bg-surface-container-lowest border border-outline-variant rounded-xl p-5">
          <span class="text-label-sm font-label-sm text-outline uppercase tracking-wider">${ui.escapeHtml(label)}</span>
          <strong class="block mt-2 text-headline-md font-headline-md text-on-surface break-words">${ui.escapeHtml(value)}</strong>
        </article>
      `).join('');
    } catch (error) {
      ui.renderMessage(target, error.message, 'error');
    }
  }

  loadStatus();
})();
