import React, { useState, useEffect, useRef } from 'react';
import './styles/theme.css';
import LoginForm from './components/LoginForm';
import RegisterForm from './components/RegisterForm';

const INITIAL_USERS = [
  {
    name: 'Ramesh Patel',
    email: 'farmer.ramesh@greenvalley.com',
    password: 'Password123!',
    phone: '+91 98160 44219',
    role: 'farmer'
  },
  {
    name: 'Chief Administrator',
    email: 'admin@freshtrace.org',
    password: 'Password123!',
    phone: '+91 98100 00001',
    role: 'admin'
  },
  {
    name: 'Ananya Sharma',
    email: 'retailer@freshmart.com',
    password: 'Password123!',
    phone: '+91 98201 55412',
    role: 'retailer'
  },
  {
    name: 'Vikram Singh',
    email: 'distributor@coldchain.com',
    password: 'Password123!',
    phone: '+91 98711 33098',
    role: 'distributor'
  },
  {
    name: 'Priya Nair',
    email: 'consumer@freshtrace.org',
    password: 'Password123!',
    phone: '+91 98450 77821',
    role: 'consumer'
  }
];

const ROLE_HOME_PAGES = {
  farmer: {
    badge: 'Farmer Portal',
    subtitle: 'Manage your harvest batches, farm parcels, and dispatch readiness.',
    ctaLabel: '+ Create Batch',
    ctaMessage: 'New harvest batch registration initiated.',
    stats: [
      { title: 'Registered Farms', value: '2 Farms', sub: 'Green Valley & Pine Ridge' },
      { title: 'Active Batches', value: '8 Batches', sub: 'Tracked in system' },
      { title: 'In Stock', value: '14,200 kg', sub: 'Ready for dispatch' },
      { title: 'Pass Rate', value: '100%', sub: 'Grade A quality' }
    ],
    tableTitle: 'Recent Harvest Batches',
    columns: ['Batch Code', 'Product', 'Origin', 'Harvest Date', 'Quantity', 'Status'],
    rows: [
      { code: 'BATCH-2026-APL-098', col2: 'Royal Delicious Apples', col3: 'Green Valley Farm', col4: 'Nov 10, 2026', col5: '2,400 kg', status: 'Ready for Dispatch', badge: 'ready' },
      { code: 'BATCH-2026-TOM-441', col2: 'Organic Vine Tomatoes', col3: 'Pine Ridge Estate', col4: 'Nov 11, 2026', col5: '1,200 kg', status: 'In Transit', badge: 'transit' },
      { code: 'BATCH-2026-CRT-204', col2: 'Himalayan Baby Carrots', col3: 'Green Valley Farm', col4: 'Nov 14, 2026', col5: '950 kg', status: 'Ready for Dispatch', badge: 'ready' }
    ]
  },
  admin: {
    badge: 'Admin Command Center',
    subtitle: 'Oversee supply chain compliance, registered stakeholders, and audit integrity.',
    ctaLabel: '+ Add Stakeholder',
    ctaMessage: 'Stakeholder onboarding workflow opened.',
    stats: [
      { title: 'Total Stakeholders', value: '148 Users', sub: 'Farmers, distributors & retailers' },
      { title: 'Active Batches', value: '312 Batches', sub: 'Live across 5 regions' },
      { title: 'Compliance Score', value: '99.4%', sub: 'SDG 12 & IEEE 830 verified' },
      { title: 'Open Alerts', value: '2 Alerts', sub: 'Minor cold-chain variance' }
    ],
    tableTitle: 'System Entities & Compliance Overview',
    columns: ['Record ID', 'Entity', 'Role', 'Region', 'Last Audit', 'Status'],
    rows: [
      { code: 'ENT-FRM-101', col2: 'Green Valley Organic Farm', col3: 'Farmer', col4: 'Himachal Pradesh', col5: 'Nov 16, 2026', status: 'Verified', badge: 'ready' },
      { code: 'ENT-DST-204', col2: 'ColdChain Reefer Fleet #04', col3: 'Distributor', col4: 'North Corridor', col5: 'Nov 17, 2026', status: 'Active Transit', badge: 'transit' },
      { code: 'ENT-RTL-309', col2: 'FreshMart Metro Superstore', col3: 'Retailer', col4: 'New Delhi', col5: 'Nov 17, 2026', status: 'Verified', badge: 'ready' },
      { code: 'ENT-ALT-012', col2: 'Solan Cold Storage Unit B', col3: 'Distributor', col4: 'Solan, HP', col5: 'Nov 18, 2026', status: 'Temp Warning', badge: 'warn' }
    ]
  },
  retailer: {
    badge: 'Retailer Storefront Hub',
    subtitle: 'Track incoming deliveries, shelf freshness scores, and store inventory.',
    ctaLabel: '+ Log Receipt',
    ctaMessage: 'Incoming shipment verification scanner opened.',
    stats: [
      { title: 'Incoming Deliveries', value: '5 Shipments', sub: '2 arriving today' },
      { title: 'Fresh Stock On Shelf', value: '6,850 kg', sub: 'Avg. 96% freshness' },
      { title: 'Expiring Soon', value: '3 Lots', sub: 'Prioritize markdown' },
      { title: 'Waste Reduced', value: '34%', sub: 'Month-over-month' }
    ],
    tableTitle: 'Store Inventory & Incoming Lots',
    columns: ['Batch Code', 'Product', 'Supplier', 'Freshness', 'Shelf Stock', 'Status'],
    rows: [
      { code: 'BATCH-2026-APL-098', col2: 'Royal Delicious Apples', col3: 'Green Valley Farm', col4: '98% Fresh', col5: '1,120 kg', status: 'On Shelf', badge: 'ready' },
      { code: 'BATCH-2026-TOM-441', col2: 'Organic Vine Tomatoes', col3: 'ColdChain Fleet #04', col4: '94% Fresh', col5: '640 kg', status: 'Arriving Today', badge: 'transit' },
      { code: 'BATCH-2026-SPN-112', col2: 'Farm-Fresh Baby Spinach', col3: 'Nashik Greens Co-op', col4: '86% Fresh', col5: '185 kg', status: 'Sell Soon', badge: 'warn' }
    ]
  },
  distributor: {
    badge: 'Distributor Cold-Chain Hub',
    subtitle: 'Monitor refrigerated fleet telemetry, active routes, and warehouse handoffs.',
    ctaLabel: '+ Dispatch Shipment',
    ctaMessage: 'New cold-chain dispatch route created.',
    stats: [
      { title: 'Active Fleet', value: '12 Trucks', sub: '100% GPS online' },
      { title: 'Avg. Reefer Temp', value: '3.8 °C', sub: 'Optimal 2–4 °C band' },
      { title: 'In-Transit Load', value: '28,400 kg', sub: 'Across 6 active routes' },
      { title: 'On-Time Delivery', value: '97.8%', sub: 'Last 30 days' }
    ],
    tableTitle: 'Live Cold-Chain Shipments',
    columns: ['Shipment ID', 'Batch & Product', 'Route', 'Reefer Temp', 'ETA', 'Status'],
    rows: [
      { code: 'SHP-8841', col2: 'Royal Delicious Apples (APL-098)', col3: 'Shimla → Delhi Hub', col4: '3.4 °C', col5: '2h 15m', status: 'In Transit', badge: 'transit' },
      { code: 'SHP-8845', col2: 'Organic Vine Tomatoes (TOM-441)', col3: 'Solan → Chandigarh', col4: '4.1 °C', col5: 'Delivered', status: 'Delivered', badge: 'ready' },
      { code: 'SHP-8849', col2: 'Baby Spinach Lots (SPN-112)', col3: 'Nashik → Mumbai Hub', col4: '5.2 °C', col5: '4h 40m', status: 'Check Cooling', badge: 'warn' }
    ]
  },
  consumer: {
    badge: 'Consumer Traceability Portal',
    subtitle: 'Verify food origin, inspect farm-to-fork journeys, and check live freshness scores.',
    ctaLabel: 'Scan QR Code',
    ctaMessage: 'Camera QR scanner ready. You can also enter a batch code below to trace provenance.',
    stats: [
      { title: 'Verified Scans', value: '14 Items', sub: '100% authentic origin' },
      { title: 'Avg. Freshness', value: '97 / 100', sub: 'Harvested within 48h' },
      { title: 'Cold-Chain Safe', value: '100%', sub: 'Zero temp breaches' },
      { title: 'Trusted Farms', value: '6 Farms', sub: 'Certified organic' }
    ],
    tableTitle: 'Recently Verified Produce',
    columns: ['Batch Code', 'Product', 'Origin Farm', 'Harvested', 'Freshness', 'Verification'],
    rows: [
      { code: 'BATCH-2026-APL-098', col2: 'Royal Delicious Apples', col3: 'Green Valley Farm, Shimla', col4: 'Nov 10, 2026', col5: '98% Fresh', status: 'Verified Origin', badge: 'ready' },
      { code: 'BATCH-2026-TOM-441', col2: 'Organic Vine Tomatoes', col3: 'Pine Ridge Estate, Solan', col4: 'Nov 11, 2026', col5: '95% Fresh', status: 'Verified Origin', badge: 'ready' },
      { code: 'BATCH-2026-CRT-204', col2: 'Himalayan Baby Carrots', col3: 'Highland Organic Co-op', col4: 'Nov 14, 2026', col5: '99% Fresh', status: 'Verified Origin', badge: 'ready' }
    ]
  }
};

const TRACEABLE_BATCHES = {
  'BATCH-2026-APL-098': 'Royal Delicious Apples • Harvested at Green Valley Farm (Shimla, HP) on Nov 10, 2026 • Cold-chain transit maintained at 3.4 °C • Verified Grade A (98% Freshness).',
  'BATCH-2026-TOM-441': 'Organic Vine Tomatoes • Harvested at Pine Ridge Estate (Solan, HP) on Nov 11, 2026 • Cold-chain transit maintained at 4.1 °C • Verified Pesticide-Free (95% Freshness).',
  'BATCH-2026-CRT-204': 'Himalayan Baby Carrots • Harvested at Highland Organic Co-op on Nov 14, 2026 • Direct chilled dispatch at 3.1 °C • Verified Organic (99% Freshness).'
};

export default function App() {
  const [currentView, setCurrentView] = useState('auth'); // 'auth' | 'dashboard' | 'profile'
  const [authMode, setAuthMode] = useState('login'); // 'login' | 'register'

  const [users, setUsers] = useState(() => {
    try {
      const stored = localStorage.getItem('freshtrace_users');
      if (stored) {
        const parsed = JSON.parse(stored);
        if (Array.isArray(parsed)) {
          const existingEmails = new Set(parsed.map((u) => u.email.toLowerCase()));
          const missingDefaults = INITIAL_USERS.filter(
            (u) => !existingEmails.has(u.email.toLowerCase())
          );
          return [...parsed, ...missingDefaults];
        }
      }
    } catch (e) {}
    return INITIAL_USERS;
  });

  useEffect(() => {
    try {
      localStorage.setItem('freshtrace_users', JSON.stringify(users));
    } catch (e) {}
  }, [users]);

  const [currentUser, setCurrentUser] = useState(null);
  const [consumerBatchQuery, setConsumerBatchQuery] = useState('BATCH-2026-APL-098');
  const [consumerTraceResult, setConsumerTraceResult] = useState('');

  // Farmer Details
  const [farmerName, setFarmerName] = useState('Ramesh Patel');
  const [phone, setPhone] = useState('+91 98160 44219');
  const [farmName, setFarmName] = useState('Green Valley Organic Farm');
  const [farmArea, setFarmArea] = useState('15.5 Acres');
  const [farmDistrict, setFarmDistrict] = useState('Shimla District, Himachal Pradesh');
  const [farmCoords, setFarmCoords] = useState('31.1048° N, 77.1734° E');
  const [isDetectingLocation, setIsDetectingLocation] = useState(false);
  const [geoStatusMsg, setGeoStatusMsg] = useState('');
  const [isFarmVisible, setIsFarmVisible] = useState(false);
  const farmSectionRef = useRef(null);

  useEffect(() => {
    if (currentView !== 'profile') return;

    const observer = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            setIsFarmVisible(true);
          }
        });
      },
      { threshold: 0.15 }
    );

    const el = farmSectionRef.current;
    if (el) observer.observe(el);

    const handleScroll = () => {
      if (el) {
        const rect = el.getBoundingClientRect();
        if (rect.top <= window.innerHeight * 0.88) {
          setIsFarmVisible(true);
        }
      }
    };

    window.addEventListener('scroll', handleScroll, { passive: true });
    handleScroll();

    return () => {
      if (el) observer.unobserve(el);
      window.removeEventListener('scroll', handleScroll);
    };
  }, [currentView]);

  const handleLoginSuccess = (user) => {
    setCurrentUser(user);
    setFarmerName(user.name);
    if (user.phone) setPhone(user.phone);
    setCurrentView('dashboard');
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  const handleRegisterSubmitted = (newUser) => {
    setUsers((prev) => [...prev, newUser]);
    setCurrentUser(newUser);
    setFarmerName(newUser.name);
    if (newUser.phone) setPhone(newUser.phone);
    setCurrentView('dashboard');
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  const handleSignOut = () => {
    setCurrentUser(null);
    setCurrentView('auth');
    setAuthMode('login');
    setConsumerTraceResult('');
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  const handleDetectCurrentLocation = () => {
    setIsDetectingLocation(true);
    if (navigator.geolocation) {
      navigator.geolocation.getCurrentPosition(
        (pos) => {
          const lat = pos.coords.latitude.toFixed(4);
          const lng = pos.coords.longitude.toFixed(4);
          const coordsStr = `${Math.abs(lat)}° ${lat >= 0 ? 'N' : 'S'}, ${Math.abs(lng)}° ${lng >= 0 ? 'E' : 'W'}`;
          setFarmCoords(coordsStr);
          setFarmDistrict('Shimla District, Himachal Pradesh');
          setIsDetectingLocation(false);
          setGeoStatusMsg(`Location detected: Shimla District (${coordsStr})`);
        },
        () => {
          setTimeout(() => {
            setFarmCoords('31.1048° N, 77.1734° E');
            setFarmDistrict('Shimla District, Himachal Pradesh');
            setIsDetectingLocation(false);
            setGeoStatusMsg('Location detected: Shimla District (31.1048° N, 77.1734° E)');
          }, 400);
        },
        { timeout: 5000, enableHighAccuracy: true }
      );
    } else {
      setFarmCoords('31.1048° N, 77.1734° E');
      setFarmDistrict('Shimla District, Himachal Pradesh');
      setIsDetectingLocation(false);
      setGeoStatusMsg('Location detected: Shimla District (31.1048° N, 77.1734° E)');
    }
  };

  const handleSaveFarmProfile = () => {
    alert('Farm details and producer profile saved successfully!');
  };

  // ==================== PROFILE VIEW ====================
  if (currentView === 'profile') {
    return (
      <div className="profile-view-wrapper" id="viewProfile" style={{ display: 'flex' }}>
        <nav className="top-nav">
          <div className="nav-brand" onClick={() => setCurrentView('dashboard')} style={{ cursor: 'pointer' }}>
            <span className="nav-brand-title">Fresh<span>Trace</span></span>
          </div>

          <div className="nav-controls">
            <button type="button" className="btn-nav-back" onClick={() => setCurrentView('dashboard')}>
              <svg className="icon-svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" style={{ width: '15px', height: '15px' }}>
                <path strokeLinecap="round" strokeLinejoin="round" d="M19 12H5M12 19l-7-7 7-7"/>
              </svg>
              <span>Back to Dashboard</span>
            </button>

            <button type="button" className="btn-nav-action" onClick={handleSignOut}>
              Sign out
            </button>
          </div>
        </nav>

        <div className="profile-container">
          <div className="profile-page-header">
            <h1>Producer Profile & Farm Registration</h1>
            <p>Manage your account credentials and farm details.</p>
          </div>

          <section className="user-details-hero-card">
            <div className="user-hero-top">
              <div className="user-brand-logo">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" style={{ width: '30px', height: '30px' }}>
                  <path strokeLinecap="round" strokeLinejoin="round" d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/>
                  <path strokeLinecap="round" strokeLinejoin="round" d="M9 12l2 2 4-4"/>
                </svg>
              </div>

              <div className="user-hero-identity">
                <h2 className="user-hero-name">{farmerName}</h2>
                <p className="user-hero-email">{currentUser ? currentUser.email : 'farmer.ramesh@greenvalley.com'}</p>
                <div className="user-badge-row">
                  <span className="verified-producer-pill">
                    <svg className="icon-svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" style={{ width: '13px', height: '13px' }}>
                      <polyline points="20 6 9 17 4 12"></polyline>
                    </svg>
                    Verified Producer
                  </span>
                </div>
              </div>
            </div>

            <div className="user-fields-grid">
              <div className="user-field-group">
                <label className="user-field-label">Full Name</label>
                <input 
                  type="text" 
                  className="user-field-input" 
                  value={farmerName}
                  onChange={(e) => setFarmerName(e.target.value)} 
                />
              </div>

              <div className="user-field-group">
                <label className="user-field-label">Email Address</label>
                <input 
                  type="email" 
                  className="user-field-input user-field-readonly" 
                  readOnly 
                  value={currentUser ? currentUser.email : 'farmer.ramesh@greenvalley.com'} 
                />
              </div>

              <div className="user-field-group">
                <label className="user-field-label">Phone</label>
                <input 
                  type="tel" 
                  className="user-field-input" 
                  value={phone}
                  onChange={(e) => setPhone(e.target.value)} 
                />
              </div>
            </div>

            <div className="scroll-down-invitation">
              <div className="scroll-prompt-text">
                <svg className="icon-svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" style={{ width: '18px', height: '18px', color: 'var(--pista-600)' }}>
                  <path strokeLinecap="round" strokeLinejoin="round" d="M19 14l-7 7m0 0l-7-7m7 7V3"></path>
                </svg>
                <span>Farm Registration located below</span>
              </div>
              <button 
                type="button" 
                className="btn-scroll-down"
                onClick={() => farmSectionRef.current?.scrollIntoView({ behavior: 'smooth' })}
              >
                <span>View Farm Details ↓</span>
              </button>
            </div>
          </section>

          <section className="farm-section-container" ref={farmSectionRef}>
            <div className={`farm-registration-card ${isFarmVisible ? 'float-right-in' : ''}`}>
              <div className="farm-card-header">
                <div>
                  <h2>
                    <svg className="icon-svg" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" d="M3 21h18M5 21V7l8-4 6 4v14M9 10h2m-2 4h2m4-4h2m-2 4h2"></path>
                    </svg>
                    Farm Details
                  </h2>
                  <p>Enter your agricultural parcel information</p>
                </div>
              </div>

              <div className="current-location-box">
                <div className="current-loc-header">
                  <div className="current-loc-title">
                    <h4>Auto-Detect Location</h4>
                    <p>Set your farm district and coordinates using GPS.</p>
                  </div>

                  <button 
                    type="button" 
                    className="btn-use-location" 
                    onClick={handleDetectCurrentLocation}
                    disabled={isDetectingLocation}
                  >
                    <svg className="icon-svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" style={{ width: '16px', height: '16px' }}>
                      <circle cx="12" cy="12" r="10"></circle>
                      <line x1="22" y1="12" x2="18" y2="12"></line>
                      <line x1="6" y1="12" x2="2" y2="12"></line>
                      <line x1="12" y1="6" x2="12" y2="2"></line>
                      <line x1="12" y1="22" x2="12" y2="18"></line>
                    </svg>
                    <span>{isDetectingLocation ? 'Detecting...' : 'Use Current Location'}</span>
                  </button>
                </div>

                {geoStatusMsg && (
                  <div className="geo-success-badge visible">
                    <svg className="icon-svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" style={{ width: '16px', height: '16px' }}>
                      <polyline points="20 6 9 17 4 12"></polyline>
                    </svg>
                    <span>{geoStatusMsg}</span>
                  </div>
                )}
              </div>

              <div className="farm-fields-grid">
                <div className="farm-field-item">
                  <label className="farm-field-label">Farm Name</label>
                  <input type="text" className="input-farm" value={farmName} onChange={(e) => setFarmName(e.target.value)} />
                </div>

                <div className="farm-field-item">
                  <label className="farm-field-label">Land Area</label>
                  <input type="text" className="input-farm" value={farmArea} onChange={(e) => setFarmArea(e.target.value)} />
                </div>

                <div className="farm-field-item">
                  <label className="farm-field-label">District / Location</label>
                  <input type="text" className="input-farm" value={farmDistrict} onChange={(e) => setFarmDistrict(e.target.value)} />
                </div>

                <div className="farm-field-item">
                  <label className="farm-field-label">Coordinates</label>
                  <input type="text" className="input-farm input-coords-readonly" value={farmCoords} onChange={(e) => setFarmCoords(e.target.value)} />
                </div>
              </div>

              <div className="farm-actions-row">
                <button type="button" className="btn-save-farm" onClick={handleSaveFarmProfile}>
                  <span>Save Farm Details</span>
                </button>
              </div>
            </div>
          </section>
        </div>
      </div>
    );
  }

  // ==================== DASHBOARD VIEW (ROLE HOME PAGES) ====================
  if (currentView === 'dashboard') {
    const activeRole = (currentUser?.role || 'farmer').toLowerCase();
    const roleConfig = ROLE_HOME_PAGES[activeRole] || ROLE_HOME_PAGES.farmer;
    const badgeClassMap = {
      ready: 'status-badge-ready',
      transit: 'status-badge-transit',
      warn: 'status-badge-warn'
    };

    const handleConsumerVerify = (e) => {
      e.preventDefault();
      const key = consumerBatchQuery.trim().toUpperCase();
      if (TRACEABLE_BATCHES[key]) {
        setConsumerTraceResult(TRACEABLE_BATCHES[key]);
      } else {
        setConsumerTraceResult(`No provenance record found for "${key}". Try BATCH-2026-APL-098, BATCH-2026-TOM-441, or BATCH-2026-CRT-204.`);
      }
    };

    return (
      <div className="dashboard-view-wrapper" id="viewDashboard" style={{ display: 'flex' }}>
        <div className="ambient-grid"></div>

        <nav className="top-nav">
          <div className="nav-brand" onClick={() => setCurrentView('dashboard')} style={{ cursor: 'pointer' }}>
            <span className="nav-brand-title">Fresh<span>Trace</span></span>
          </div>

          <div className="nav-controls">
            <button 
              type="button" 
              className="btn-profile-trigger" 
              onClick={() => {
                setCurrentView('profile');
                window.scrollTo({ top: 0, behavior: 'smooth' });
              }}
              title="Account Profile"
            >
              <span className="avatar-icon-wrap">
                <svg className="icon-svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" style={{ width: '14px', height: '14px' }}>
                  <path strokeLinecap="round" strokeLinejoin="round" d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"></path>
                  <circle cx="12" cy="7" r="4"></circle>
                </svg>
              </span>
              <span>{currentUser ? currentUser.name : farmerName}</span>
            </button>

            <button type="button" className="btn-nav-action" onClick={handleSignOut}>
              Sign out
            </button>
          </div>
        </nav>

        <main className="dash-content">
          <div className="dash-greeting">
            <div>
              <span className="role-pill-badge">{roleConfig.badge}</span>
              <h1>Welcome, {currentUser ? currentUser.name : farmerName}</h1>
              <p>Role: <strong style={{ textTransform: 'capitalize' }}>{activeRole}</strong> • {roleConfig.subtitle}</p>
            </div>
            <button 
              type="button" 
              className="btn-create-batch"
              onClick={() => alert(roleConfig.ctaMessage)}
            >
              <span>{roleConfig.ctaLabel}</span>
            </button>
          </div>

          <div className="stats-grid">
            {roleConfig.stats.map((stat, idx) => (
              <div className="stat-card" key={idx}>
                <div className="stat-title">{stat.title}</div>
                <div className="stat-val">{stat.value}</div>
                <div className="stat-sub">{stat.sub}</div>
              </div>
            ))}
          </div>

          {activeRole === 'consumer' && (
            <div className="consumer-verify-card">
              <div className="table-header-row" style={{ marginBottom: 0 }}>
                <h2 className="table-title">Instant Batch Origin Lookup</h2>
              </div>
              <form className="consumer-verify-row" onSubmit={handleConsumerVerify}>
                <input
                  type="text"
                  className="consumer-verify-input"
                  placeholder="Enter Batch Code (e.g. BATCH-2026-APL-098)"
                  value={consumerBatchQuery}
                  onChange={(e) => setConsumerBatchQuery(e.target.value)}
                />
                <button type="submit" className="btn-create-batch">
                  <span>Verify Batch</span>
                </button>
              </form>
              {consumerTraceResult && (
                <div className="consumer-trace-result">{consumerTraceResult}</div>
              )}
            </div>
          )}

          <div className="table-card">
            <div className="table-header-row">
              <h2 className="table-title">{roleConfig.tableTitle}</h2>
            </div>

            <table className="batches-table">
              <thead>
                <tr>
                  {roleConfig.columns.map((col, idx) => (
                    <th key={idx}>{col}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {roleConfig.rows.map((row, idx) => (
                  <tr key={idx}>
                    <td><span className="batch-id-pill">{row.code}</span></td>
                    <td><strong>{row.col2}</strong></td>
                    <td>{row.col3}</td>
                    <td>{row.col4}</td>
                    <td>{row.col5}</td>
                    <td><span className={badgeClassMap[row.badge] || 'status-badge-ready'}>{row.status}</span></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </main>
      </div>
    );
  }

  // ==================== AUTH VIEW (LOGIN & REGISTER) ====================
  return (
    <div className="signin-view-wrapper" id="viewAuth">
      <div className="ambient-grid"></div>

      {/* Animated Farm-to-Fork Truck Transit Background */}
      <div className="truck-transit-backdrop" aria-hidden="true">
        <div className="transit-sky">
          {/* Day / Night Atmospheric Sky Layers */}
          <div className="sky-night-backdrop"></div>

          {/* Twinkling Stars (Night Sky) */}
          <div className="transit-stars">
            <div className="star star-1"></div>
            <div className="star star-2"></div>
            <div className="star star-3"></div>
            <div className="star star-4"></div>
            <div className="star star-5"></div>
            <div className="star star-6"></div>
            <div className="star star-7"></div>
            <div className="star star-8"></div>
            <div className="star star-9"></div>
            <div className="star star-10"></div>
          </div>

          {/* Alternating Sun & Moon */}
          <div className="transit-sun">
            <div className="sun-corona"></div>
          </div>

          <div className="transit-moon">
            <div className="moon-crater crater-1"></div>
            <div className="moon-crater crater-2"></div>
            <div className="moon-crater crater-3"></div>
            <div className="moon-halo"></div>
          </div>

          {/* Drifting Clouds */}
          <div className="transit-cloud cloud-1"></div>
          <div className="transit-cloud cloud-2"></div>
          <div className="transit-cloud cloud-3"></div>
        </div>

        <div className="transit-hills">
          <svg className="hills-svg" viewBox="0 0 1440 180" preserveAspectRatio="none">
            <path fill="#e6f2e1" d="M0,80 C240,140 480,40 720,80 C960,120 1200,50 1440,90 L1440,180 L0,180 Z"></path>
            <path fill="#d8ebce" d="M0,110 C320,60 640,140 960,100 C1200,70 1340,130 1440,120 L1440,180 L0,180 Z"></path>
          </svg>
        </div>

        {/* Location 1: Farm (Left) */}
        <div className="transit-location location-origin">
          <div className="farm-structures">
            <div className="farm-windmill">
              <div className="windmill-pillar"></div>
              <div className="windmill-hub"></div>
              <div className="windmill-blades">
                <div className="blade blade-1"></div>
                <div className="blade blade-2"></div>
                <div className="blade blade-3"></div>
                <div className="blade blade-4"></div>
              </div>
            </div>
            <div className="farm-barn">
              <div className="barn-roof"></div>
              <div className="barn-body">
                <div className="barn-door"></div>
              </div>
            </div>
            <div className="farm-silo"></div>
            <div className="farm-tree tree-1"></div>
            <div className="farm-tree tree-2"></div>
          </div>
        </div>

        {/* Location 2: Destination Hub (Right) */}
        <div className="transit-location location-destination">
          <div className="city-structures">
            <div className="city-building building-1">
              <div className="solar-roof"></div>
              <div className="building-windows"></div>
            </div>
            <div className="city-building building-2">
              <div className="warehouse-accent"></div>
              <div className="building-windows"></div>
            </div>
            <div className="city-building building-3">
              <div className="building-windows"></div>
            </div>
          </div>
        </div>

        {/* Running Delivery Truck */}
        <div className="animated-truck-track">
          <div className="running-truck">
            <div className="gps-signal-wave">
              <div className="signal-ring ring-1"></div>
              <div className="signal-ring ring-2"></div>
            </div>

            <div className="truck-body-wrap">
              <div className="truck-trailer">
                <div className="reefer-unit"></div>
                <div className="trailer-accent-stripe"></div>
              </div>

              <div className="truck-cab">
                <div className="cab-windshield"></div>
                <div className="cab-headlight"></div>
                <div className="headlight-beam"></div>
              </div>
            </div>

            <div className="truck-wheels">
              <div className="wheel wheel-rear">
                <div className="wheel-rim"></div>
              </div>
              <div className="wheel wheel-mid">
                <div className="wheel-rim"></div>
              </div>
              <div className="wheel wheel-front">
                <div className="wheel-rim"></div>
              </div>
            </div>

            <div className="truck-exhaust-puff"></div>
          </div>
        </div>
      </div>

      <div className="login-container">
        <div className="login-card">
          {/* Tabs to select between Login and Register */}
          <div className="auth-mode-switch" role="tablist">
            <button
              type="button"
              className={`auth-mode-tab ${authMode === 'login' ? 'active' : ''}`}
              onClick={() => setAuthMode('login')}
            >
              <span>Sign In</span>
            </button>

            <button
              type="button"
              className={`auth-mode-tab ${authMode === 'register' ? 'active' : ''}`}
              onClick={() => setAuthMode('register')}
            >
              <span>Register</span>
            </button>
          </div>

          {authMode === 'login' ? (
            <LoginForm 
              onLoginSuccess={handleLoginSuccess}
              existingUsers={users}
            />
          ) : (
            <RegisterForm 
              onRegisterSubmitted={handleRegisterSubmitted}
              existingUsers={users}
            />
          )}
        </div>
      </div>
    </div>
  );
}
