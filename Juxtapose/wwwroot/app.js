(() => {
  // ---------- Bridge RPC layer ----------
  const pending = new Map();
  let requestCounter = 0;

  function callBridge(action, payload) {
    return new Promise((resolve, reject) => {
      const requestId = `req_${++requestCounter}`;
      pending.set(requestId, { resolve, reject });
      window.chrome.webview.postMessage(JSON.stringify({ requestId, action, payload }));
    });
  }

  window.chrome.webview.addEventListener('message', (event) => {
    let msg = event.data;
    if (typeof msg === 'string') {
      try { msg = JSON.parse(msg); } catch { return; }
    }

    if (msg.type === 'log') {
      appendLog(msg.message);
      return;
    }
    if (msg.type === 'progress') {
      setProgress(msg.percent);
      return;
    }
    if (msg.requestId && pending.has(msg.requestId)) {
      const { resolve, reject } = pending.get(msg.requestId);
      pending.delete(msg.requestId);
      if (msg.success) {
        resolve(msg.data);
      } else {
        reject(new Error(msg.error || 'Unknown error'));
      }
    }
  });

  // ---------- Tab handling ----------
  document.querySelectorAll('.tab-header').forEach((btn) => {
    btn.addEventListener('click', () => {
      document.querySelectorAll('.tab-header').forEach((b) => b.classList.remove('active'));
      document.querySelectorAll('.tab-page').forEach((p) => p.classList.remove('active'));
      btn.classList.add('active');
      document.getElementById(btn.dataset.tab).classList.add('active');
    });
  });

  // ---------- Log & progress ----------
  const txtLog = document.getElementById('txtLog');
  function appendLog(message) {
    txtLog.value += (txtLog.value ? '\n' : '') + message;
    txtLog.scrollTop = txtLog.scrollHeight;
  }

  const progressBar1 = document.getElementById('progressBar1');
  function setProgress(percent) {
    progressBar1.value = percent;
  }

  // ---------- Left/Right path text boxes ----------
  const txtLeft = document.getElementById('txtLeft');
  const txtRight = document.getElementById('txtRight');
  const drpBase = document.createElement('select'); // hidden helper, not shown in original grouping but kept for baseFolder text
  drpBase.style.display = 'none';
  document.body.appendChild(drpBase);

  txtLeft.addEventListener('input', () => {
    const text = txtLeft.value;
    drpBase.innerHTML = '';
    if (text.includes('IonicMas')) {
      ['mas/src', 'mas/MlineParameters', 'mas/ReportMlineTemplates', 'ionic-client'].forEach((v) => {
        const opt = document.createElement('option');
        opt.value = v;
        opt.textContent = v;
        drpBase.appendChild(opt);
      });
      drpBase.selectedIndex = 0;
    } else if (text.includes('Client')) {
      ['src', 'images'].forEach((v) => {
        const opt = document.createElement('option');
        opt.value = v;
        opt.textContent = v;
        drpBase.appendChild(opt);
      });
      drpBase.selectedIndex = 0;
    }
  });

  function getBaseFolder() {
    return drpBase.value || '';
  }

  // ---------- SVN tree rendering ----------
  function buildTreeHtml(node, depth) {
    const li = document.createElement('li');
    li.className = 'tree-node';

    const hasChildren = !!(node.children && node.children.length > 0);

    const label = document.createElement('span');
    label.className = 'node-label';

    const toggle = document.createElement('span');
    toggle.className = hasChildren ? 'node-toggle' : 'node-toggle node-toggle-empty';
    if (hasChildren) {
      toggle.textContent = depth >= 1 ? '\u25B8' : '\u25BE'; // collapsed vs expanded arrow
    }
    label.appendChild(toggle);

    const icon = document.createElement('span');
    icon.className = 'node-icon';
    label.appendChild(icon);

    const text = document.createElement('span');
    text.className = 'node-text';
    text.textContent = node.name;
    label.appendChild(text);

    li.appendChild(label);

    if (hasChildren) {
      const ul = document.createElement('ul');
      // Collapse everything below the first level under the SVN root.
      if (depth >= 1) {
        ul.classList.add('collapsed');
      }
      node.children.forEach((child) => ul.appendChild(buildTreeHtml(child, depth + 1)));
      li.appendChild(ul);

      toggle.addEventListener('click', (e) => {
        e.stopPropagation();
        const collapsed = ul.classList.toggle('collapsed');
        toggle.textContent = collapsed ? '\u25B8' : '\u25BE';
      });
    }

    label.addEventListener('click', (e) => {
      e.stopPropagation();
      const container = li.closest('.tree');
      container.querySelectorAll('.tree-node.selected').forEach((n) => n.classList.remove('selected'));
      li.classList.add('selected');

      const fullPath = getFullPath(li);
      if (container.id === 'treeLeft') {
        txtLeft.value = fullPath;
        txtLeft.dispatchEvent(new Event('input'));
      } else if (container.id === 'treeRight') {
        txtRight.value = fullPath;
      }
    });

    return li;
  }

  function getFullPath(li) {
    const parts = [];
    let current = li;
    while (current && current.classList && current.classList.contains('tree-node')) {
      const text = current.querySelector(':scope > .node-label > .node-text');
      parts.unshift(text.textContent);
      const parentUl = current.parentElement;
      current = parentUl && parentUl.classList.contains('tree') ? null : parentUl?.closest('.tree-node') ?? null;
    }
    return parts.join('/');
  }

  function renderTree(containerId, node) {
    const container = document.getElementById(containerId);
    container.innerHTML = '';
    const rootUl = document.createElement('ul');
    rootUl.appendChild(buildTreeHtml(node, 0));
    container.appendChild(rootUl);
  }

  document.getElementById('btnLoadSVN').addEventListener('click', async () => {
    const svnRootUrl = document.getElementById('txtSVNRoot').value;

    const confirmed = confirm(
      'Loading the SVN hierarchy will overwrite the previously cached tree structure.\n\nDo you want to continue?'
    );
    if (!confirmed) {
      return;
    }

    try {
      const tree = await callBridge('loadSvnHierarchy', { svnRootUrl, maxLevels: 3 });
      renderTree('treeLeft', tree);
      renderTree('treeRight', tree);
    } catch (err) {
      alert(`Failed to load SVN hierarchy: ${err.message}`);
    }
  });

  // On startup, try to load a previously cached SVN hierarchy so the trees are
  // populated immediately without requiring the user to click "Load" again.
  (async () => {
    try {
      const cachedTree = await callBridge('loadCachedSvnHierarchy', {});
      if (cachedTree) {
        renderTree('treeLeft', cachedTree);
        renderTree('treeRight', cachedTree);
      }
    } catch (err) {
      // No cache yet, or failed to load it - nothing to do, user can click Load.
    }
  })();

  // ---------- Analyze ----------
  let currentRows = [];

  document.getElementById('btnAnalyze').addEventListener('click', async () => {
    const confirmedAnalyze = confirm(
      'This will start the analysis using the current left/right paths and may perform SVN checkout/update.\n\nDo you want to continue?'
    );
    if (!confirmedAnalyze) {
      return;
    }

    setProgress(0);
    const leftPath = txtLeft.value;
    const rightPath = txtRight.value;
    const baseFolder = getBaseFolder();
    const extensionsCsv = document.getElementById('txtExtensions').value;

    try {
      const result = await callBridge('analyze', {
        leftPath,
        rightPath,
        baseFolder,
        extensionsCsv,
        performSvnUpdate: true
      });
      currentRows = result.rows || [];
      renderGrid(currentRows);
      populateUsersFromRevisions();
    } catch (err) {
      alert(`Analysis failed: ${err.message}`);
    }
  });

  // ---------- Grid rendering ----------
  const gridViewBody = document.getElementById('gridViewBody');

  function renderGrid(rows) {
    gridViewBody.innerHTML = '';
    rows.forEach((row) => {
      const tr = document.createElement('tr');
      tr.dataset.status = row.status;
      tr.dataset.revisions = row.revisions || '';

      const cells = [
        row.status, row.left, row.right, row.added, row.deleted,
        row.modified, row.total, row.changePercent, row.revisions
      ];

      cells.forEach((val, idx) => {
        const td = document.createElement('td');
        td.textContent = val ?? '';
        if (idx === 0) {
          td.className = `status-${row.status}`;
        }
        tr.appendChild(td);
      });

      tr.addEventListener('dblclick', (e) => onGridDoubleClick(e, row));
      tr.addEventListener('contextmenu', (e) => onGridContextMenu(e, tr, row));
      tr.addEventListener('click', () => {
        gridViewBody.querySelectorAll('tr.selected').forEach((r) => r.classList.remove('selected'));
        tr.classList.add('selected');
      });

      gridViewBody.appendChild(tr);
    });

    if (document.getElementById('chkAutoScroll').checked) {
      const wrapper = document.querySelector('.grid-wrapper');
      wrapper.scrollTop = wrapper.scrollHeight;
    }
  }

  async function onGridDoubleClick(e, row) {
    const cellIndex = Array.from(e.currentTarget.children).indexOf(e.target);
    if (cellIndex !== 1 && cellIndex !== 2) return;

    const cellValue = cellIndex === 1 ? row.left : row.right;
    if (!cellValue) return;

    if (e.ctrlKey) {
      const svnUrl = cellIndex === 1
        ? `${txtLeft.value}/${cellValue}`
        : `${txtRight.value}/${cellValue}`;

      const tortoiseSvnPath = document.getElementById('txtSVN').value;
      try {
        await callBridge('showSvnLog', { tortoiseSvnPath, svnUrl });
      } catch (err) {
        appendLog(`Failed to show SVN log. Error: ${err.message}`);
      }
      return;
    }

    const leftFull = `${txtLeft.value}/${row.left}`;
    const rightFull = `${txtRight.value}/${row.right}`;
    try {
      await callBridge('openWinMerge', {
        winMergeToolPath: document.getElementById('txtWinmerge').value,
        leftPath: leftFull,
        rightPath: rightFull
      });
    } catch (err) {
      appendLog(`Failed to compare with WinMerge. Error: ${err.message}`);
    }
  }

  // ---------- Context menu ----------
  const contextMenu = document.getElementById('contextMenu');
  let contextRow = null;

  function onGridContextMenu(e, tr, row) {
    e.preventDefault();
    contextRow = row;
    gridViewBody.querySelectorAll('tr.selected').forEach((r) => r.classList.remove('selected'));
    tr.classList.add('selected');
    contextMenu.style.left = `${e.pageX}px`;
    contextMenu.style.top = `${e.pageY}px`;
    contextMenu.style.display = 'block';
  }

  document.addEventListener('click', () => {
    contextMenu.style.display = 'none';
  });

  contextMenu.addEventListener('click', async (e) => {
    const action = e.target.dataset.action;
    if (!action || !contextRow) return;

    const leftFull = `${txtLeft.value}/${contextRow.left}`;
    const rightFull = `${txtRight.value}/${contextRow.right}`;

    try {
      switch (action) {
        case 'compareSVN':
          await callBridge('openSvnDiff', {
            tortoiseSvnPath: document.getElementById('txtSVN').value,
            leftPath: leftFull,
            rightPath: rightFull
          });
          break;
        case 'showSvnDiff':
          await callBridge('openSvnDiffNotepad', { leftPath: leftFull, rightPath: rightFull });
          break;
        case 'compareWinMerge':
          await callBridge('openWinMerge', {
            winMergeToolPath: document.getElementById('txtWinmerge').value,
            leftPath: leftFull,
            rightPath: rightFull
          });
          break;
        case 'showSvnLog': {
          const svnUrl = contextRow.right ? rightFull : leftFull;
          await callBridge('showSvnLog', {
            tortoiseSvnPath: document.getElementById('txtSVN').value,
            svnUrl
          });
          break;
        }
        case 'exportExcel': {
          const result = await callBridge('exportExcel', { rows: currentRows });
          appendLog(result.message);
          break;
        }
        case 'exportHtml': {
          const result = await callBridge('exportHtml', {
            rows: currentRows,
            leftText: txtLeft.value,
            rightText: txtRight.value
          });
          appendLog(`Exported HTML report to ${result.filePath}`);
          break;
        }
        case 'exportTsv': {
          const result = await callBridge('exportTsv', {
            rows: currentRows,
            directory: 'Reports',
            fileNamePrefix: 'Export'
          });
          appendLog(`Exported TSV to ${result.filePath}`);
          break;
        }
        case 'exportSummary': {
          const result = await callBridge('exportSummaryTsv', { rows: currentRows });
          appendLog(`Exported summary to ${result.filePath}`);
          break;
        }
      }
    } catch (err) {
      appendLog(`Action '${action}' failed: ${err.message}`);
    }
  });

  // ---------- Filtering ----------
  document.getElementById('btnFilter').addEventListener('click', () => {
    const selectedUser = document.getElementById('drpUser').value;
    const selectedStatuses = [];
    if (document.getElementById('chkAdded').checked) selectedStatuses.push('ADDED');
    if (document.getElementById('chkDeleted').checked) selectedStatuses.push('DELETED');
    if (document.getElementById('chkMoved').checked) selectedStatuses.push('MOVED');
    if (document.getElementById('chkModified').checked) selectedStatuses.push('MODIFIED');
    if (document.getElementById('chkIdentical').checked) selectedStatuses.push('IDENTICAL');

    gridViewBody.querySelectorAll('tr').forEach((tr) => {
      const status = tr.dataset.status;
      const revisions = tr.dataset.revisions || '';

      const isStatusMatch = selectedStatuses.includes(status);
      let isUserMatch = true;
      if (selectedUser && selectedUser !== 'All Users') {
        isUserMatch = revisions.includes(selectedUser);
      }

      tr.style.display = (isStatusMatch && isUserMatch) ? '' : 'none';
    });
  });

  document.getElementById('btnBackup').addEventListener('click', async () => {
    try {
      const picked = await callBridge('pickSaveFile', { filter: 'TSV files (*.tsv)|*.tsv|All files (*.*)|*.*', fileName: 'backup.tsv' });
      if (!picked.filePath) return;
      const result = await callBridge('backup', { rows: currentRows, filePath: picked.filePath });
      appendLog(`Backed up to ${result.filePath}`);
    } catch (err) {
      appendLog(`Backup failed: ${err.message}`);
    }
  });

  document.getElementById('btnImport').addEventListener('click', async () => {
    try {
      const picked = await callBridge('pickOpenFile', { filter: 'TSV files (*.tsv)|*.tsv|All files (*.*)|*.*' });
      if (!picked.filePath) return;
      const result = await callBridge('restore', { filePath: picked.filePath });
      currentRows = result.rows || [];
      renderGrid(currentRows);
      populateUsersFromRevisions();
    } catch (err) {
      appendLog(`Restore failed: ${err.message}`);
    }
  });

  // ---------- User dropdown population ----------
  function populateUsersFromRevisions() {
    const drpUser = document.getElementById('drpUser');
    drpUser.innerHTML = '';
    const allOpt = document.createElement('option');
    allOpt.textContent = 'All Users';
    drpUser.appendChild(allOpt);

    const uniqueUsers = new Set();

    currentRows.forEach((row) => {
      if (!row.revisions) return;
      const entries = row.revisions.split('~~~~~~');
      entries.forEach((entry) => {
        const fields = entry.split(',');
        if (fields.length >= 5) {
          const userName = fields[2].trim();
          if (userName) uniqueUsers.add(userName);
        }
      });
    });

    Array.from(uniqueUsers).sort().forEach((user) => {
      const opt = document.createElement('option');
      opt.textContent = user;
      drpUser.appendChild(opt);
    });

    drpUser.selectedIndex = 0;
  }
})();
