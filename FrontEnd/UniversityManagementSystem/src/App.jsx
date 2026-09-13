import React from 'react'
import Login from './pages/Login'
import DashBoard from './pages/Dashboards/AdminDashBoard'

import { Routes, Route } from 'react-router-dom'

const App = () => {
  return (
    <Routes>
      <Route path="/" element={<Login/>}/>
      <Route path="/dashboard" element={<DashBoard/>} />
      
    </Routes>
  )
}

export default App