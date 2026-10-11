import { useState } from 'react';
import { Link, Route, Routes } from 'react-router-dom';
import EventList from './pages/EventList.jsx';
import EventDetail from './pages/EventDetail.jsx';
import CreateEvent from './pages/CreateEvent.jsx';
import MyBookings from './pages/MyBookings.jsx';

export default function App() {
  const [userId, setUserId] = useState(localStorage.getItem('userId') || '');

  const changeUser = (v) => {
    setUserId(v);
    localStorage.setItem('userId', v);
  };

  return (
    <>
      <header>
        <Link to="/" className="brand">Yaba</Link>
        <nav>
          <Link to="/">Events</Link>
          <Link to="/create">Create event</Link>
          <Link to="/bookings">My bookings</Link>
        </nav>
        <input
          placeholder="Your user id"
          value={userId}
          onChange={(e) => changeUser(e.target.value)}
        />
      </header>
      <main>
        <Routes>
          <Route path="/" element={<EventList />} />
          <Route path="/events/:id" element={<EventDetail userId={userId} />} />
          <Route path="/create" element={<CreateEvent />} />
          <Route path="/bookings" element={<MyBookings userId={userId} />} />
        </Routes>
      </main>
    </>
  );
}
