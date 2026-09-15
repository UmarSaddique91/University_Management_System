import React from 'react'
import Login from "./pages/Auth/Login"
import DashBoard from './pages/Dashboards/AdminDashBoard'
import ForgotPassword from './pages/Auth/ForgotPassword'
import ResetPassword from './pages/Auth/ResetPassword'
import SignUp from './pages/Auth/SignUp'

import { Routes, Route } from 'react-router-dom'

const App = () => {
  return (
    <Routes>
      <Route path="/" element={<Login/>}/>
      <Route path="/dashboard" element={<DashBoard/>} />
      <Route path='/forgotPassword' element={<ForgotPassword/>}/>
      <Route path='/resetPassword' element={<ResetPassword/>}/>
      <Route path='/signUp' element={<SignUp/>}/>
      
    </Routes>
  )
}

export default App