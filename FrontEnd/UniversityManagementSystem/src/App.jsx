import React from 'react'
import Login from "./pages/Auth/Login"
import DashBoard from './pages/Dashboards/AdminDashBoard'
import ForgotPassword from './pages/Auth/ForgotPassword'

import { Routes, Route } from 'react-router-dom'

const App = () => {
  return (
    <Routes>
      <Route path="/" element={<Login/>}/>
      <Route path="/dashboard" element={<DashBoard/>} />
      <Route path='/forgotPassword' element={<ForgotPassword/>}/>
      
    </Routes>
  )
}

export default App