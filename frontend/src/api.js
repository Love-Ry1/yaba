async function request(url, options) {
  const res = await fetch(url, {
    headers: { 'Content-Type': 'application/json' },
    ...options,
  });
  if (!res.ok) {
    let msg = `Request failed (${res.status})`;
    try {
      const body = await res.json();
      msg = body.title || (body.errors && Object.values(body.errors).flat().join(' ')) || msg;
    } catch {}
    throw new Error(msg);
  }
  return res.status === 204 ? null : res.json();
}

export const listEvents = (page = 1) => request(`/api/events?page=${page}&pageSize=20`);
export const getEvent = (id) => request(`/api/events/${id}`);
export const createEvent = (data) => request('/api/events', { method: 'POST', body: JSON.stringify(data) });
export const getAvailability = (id) => request(`/api/bookings/${id}/availability`);
export const reserve = (eventId, userId, seats) =>
  request('/api/bookings', { method: 'POST', body: JSON.stringify({ eventId, userId, seats }) });
export const listBookings = (userId) => request(`/api/bookings?userId=${encodeURIComponent(userId)}`);
export const cancelBooking = (id) => request(`/api/bookings/${id}`, { method: 'DELETE' });
