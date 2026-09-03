import React from 'react'
import {useState} from 'react'
import loginImg from '../assets/loginImg.png'
import {useNavigate} from 'react-router-dom';


const Login = () => {

  const navigate=useNavigate();

  const [email,setEmail]= useState('');
  const [password,setPassword]=useState('');

  const handleLogin=(e)=>{
    e.preventDefault();

    console.log("Submitted Button Clicked");
    console.log("Email:",email);
    console.log("Password:",password)

    navigate('/dashboard')
  }

  
  return (
       <div className="w-full min-h-screen bg-white flex">

        {/* Left SAECTION */}
        <div className="w-1/2 bg-white md:flex items-center justify-center">
        <img src={loginImg} 
        alt="UMS Login Illustration" 
        className="object-cover h-full"/>
        </div>

        {/* Right Side Section */}

        <div className="w-1/2 bg-white flex items-center justify-center shadow-lg ">
        <div className="bg-white border border-gray-200 rounded-lg shadow-lg p-8 w-full max-w-md overflow-hidden">
            <h2 className="text-2xl font-bold mb-2">Welcome Back!</h2>
            <p className="text-slate-500">Sign in to access your UMS Dashboard</p>

            {/* Form  */}
            <div>
              <form onSubmit={handleLogin}className="mt-6 space-y-4">

                {/* Email Address */}
                <div>
                  <label htmlFor="" className="font-semibold">Email Address</label>
                  <input type="email"
                  value={email}
                  onChange={(e)=>setEmail(e.target.value)}
                  placeholder="Enter your email"
                  className="w-full px-4 py-2 mt-2 border border-gray-800 hover:border-gray-300 focus rounded-md focus:outline-none  "
                  />
                </div>

                {/* Password  */}
                <div>
                  <label htmlFor="" className="font-semibold">Password</label>
                  <input type="password"
                  value={password}
                  onChange={(e)=>setPassword(e.target.value)}
                  placeholder="Enter your password"
                  className="w-full px-4 py-2 my-2 border border-gray-800 hover:border-gray-300 focus rounded-md docus:outline-none"
                  />
                </div>

                <div className="flex justify-between items-center mt-4 ">
                <label htmlFor="" className="flex items-center cursor-pointer" >
                  <input type="checkbox" className='mr-2 '/>
                  Keep me signed in
                </label>

                <span className="text-blue-500 hover:text-blue-700 cursor-pointer  ">Forgot Password?</span>
              </div>
              
              <button type="submit" className="w-full bg-blue-500 hover:bg-blue-600 text-center py-3 cursor-pointer rounded rounded-lg mt-6 text-white">
                Sign In
                </button>
                
              </form>

              
            </div>


            {/* Support */}

            <div className="bg-gray-200 -mx-8 -mb-8 py-6 flex items-center justify-center mt-5">
            <span className="text-slate-500">Need Help?</span>
            <a href="mailto:support@university.com" className="text-blue-500 cursor-pointer hover:text-blue-700">Contact It Support</a>
            </div>


        </div>
        </div>

    </div>
  )
}

export default Login